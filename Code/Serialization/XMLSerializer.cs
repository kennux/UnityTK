using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityTK.Serialization.XML;

namespace UnityTK.Serialization
{
	public struct XMLSerializerParams
	{
		public string standardNamespace;
		public string rootElementName;
	}

	public class XMLSerializer : SerializerBase<XMLSerializerParams>
	{
		public const string AttributeInherits = "Inherits";
		public const string AttributeIdentifier = "Id";
		public const string AttributeType = "Type";
		public const string AttributeAbstract = "Abstract";
		public const string AttributeCollectionOverrideAction = "CollectionOverrideAction";

		public const string TokenNull = "$NULL";

		public XMLSerializer(XMLSerializerParams parameters) : base(parameters) { }

		private Dictionary<SerializedData, string> filenameMappings = new Dictionary<SerializedData, string>();
		private List<ISerializableRoot> allParsedObjects = new List<ISerializableRoot>();
		private Dictionary<string, SerializedData> serializedData = new Dictionary<string, SerializedData>();
		private void _PreParse(string xmlContent, string filename, XMLSerializerParams parameters, List<SerializedData> result, Dictionary<SerializedData, string> filenameMappings, List<SerializerError> errors)
		{
			var xElement = XElement.Parse(xmlContent);

			// Validity checks
			if (!SerializerValidation.ContainerElementName(parameters, xElement, filename, errors))
				return;
			
			// Iterate over nodes
			foreach (var xNode in xElement.Nodes())
			{
				var nodeXElement = xNode as XElement;

				// Validity checks
				if (!SerializerValidation.NodeIsElement(parameters, xNode, filename, errors))
					continue;

				var elementType = SerializerCache.GetSerializableTypeCacheFor(nodeXElement.Name.LocalName, parameters.standardNamespace);
				if (!SerializerValidation.RootTypeFound(parameters, nodeXElement.Name.LocalName, nodeXElement, elementType, filename, errors))
					continue;

				// Prepare
				var data = new SerializedData(elementType, nodeXElement);
				result.Add(data);
				filenameMappings.Set(data, filename);
			}
		}

		public override void Deserialize(string[] data, string[] filenames, List<ISerializableRoot> externalReferenceables, out List<ISerializableRoot> parsedObjects, out List<SerializerError> errors)
		{
			if (externalReferenceables == null)
				externalReferenceables = new List<ISerializableRoot>();

			parsedObjects = new List<ISerializableRoot>();
			List<SerializedData> serializedData = ListPool<SerializedData>.Get();
			errors = ListPool<SerializerError>.Get();

			List<ISerializableRoot> allReferenceables = new List<ISerializableRoot>(externalReferenceables);

			for (int i = 0; i < data.Length; i++)
			{
				_PreParse(data[i], filenames[i], parameters, serializedData, filenameMappings, errors);
			}
			
			// Get prototypes with others inheriting from first

			// Key = type which is inheriting from something, Value = the type its inheriting from
			Dictionary<SerializedData, List<SerializedData>> inheritingFrom = new Dictionary<SerializedData, List<SerializedData>>(); // This is only used for topo sort!
			Dictionary<SerializedData, object> instances = new Dictionary<SerializedData, object>();
			Dictionary<string, SerializedData> idMapping = new Dictionary<string, SerializedData>();
			List<SerializedData> invalid = new List<SerializedData>();
			

			// Pre-parse names, create instances and apply name
			foreach (var d in serializedData)
			{
				// Check if abstract prototype data
				var attribAbstract = d.xElement.Attribute(AttributeAbstract);
				bool isAbstract = !ReferenceEquals(attribAbstract, null) && string.Equals("True", attribAbstract.Value);

				// Read name
				var attribId = d.xElement.Attribute(AttributeIdentifier);
				if (isAbstract && attribId == null)
					errors.Add(new SerializerError(SerializerErrorSeverity.WARNING, filenameMappings[d], (d.xElement as IXmlLineInfo).LineNumber, "Abstract object doesn't have an Id. This doesnt make sense!"));
				
				if (attribId != null)
					idMapping.Add(attribId.Value, d);

				if (!isAbstract)
				{
					var obj = d.targetType.Create();
					instances.Add(d, obj);

					if (attribId != null)
						(obj as ISerializableRoot).identifier = attribId.Value;

					parsedObjects.Add(obj as ISerializableRoot);
					allParsedObjects.Add(obj as ISerializableRoot);
					allReferenceables.Add(obj as ISerializableRoot);
				}
			}
			
			// Remove invalidated entries
			foreach (var d in invalid)
				serializedData.Remove(d);

			invalid.Clear();
			foreach (var d in serializedData)
			{
				SerializedData inheritedData;
				if (!string.IsNullOrEmpty(d.inherits) && idMapping.TryGetValue(d.inherits, out inheritedData))
					inheritingFrom.GetOrCreate(d).Add(inheritedData);
			}

			// Step 1 - sort by inheritance
			List<SerializedData> empty = new List<SerializedData>();
			var sorted = serializedData.TSort((sd) => inheritingFrom.ContainsKey(sd) ? inheritingFrom[sd] : empty, true).ToList();

			// Step 2 - Preloads the fields and creates sub-data objects
			foreach (var d in sorted)
				d.LoadFieldsFromXML(filenameMappings[d], errors, parameters);

			// Step 3 - run sorting algorithm for reference resolve
			foreach (var d in serializedData)
				d.ResolveReferenceFields(filenameMappings[d], allReferenceables, errors, parameters);

			// Step 4 - Final data apply
			List<SerializedData> inheritingFromTmp = new List<SerializedData>();
			foreach (var d in sorted)
			{
				if (!instances.ContainsKey(d))
					continue;

				// Apply inherited data first
				if (!string.IsNullOrEmpty(d.inherits))
				{
					// Look up all inherited data in bottom to top order
					inheritingFromTmp.Clear();
					var inheritedData = d.inherits;

					while (!string.IsNullOrEmpty(inheritedData))
					{
						SerializedData _serializedData = null;
						if (!this.serializedData.TryGetValue(inheritedData, out _serializedData) && !idMapping.TryGetValue(inheritedData, out _serializedData))
							errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filenameMappings[d], (d.xElement as IXmlLineInfo).LinePosition, "Could not find the root type '" + inheritedData + "' for object '" + (instances[d] as ISerializableRoot).identifier + "'! Ignoring inheritance!"));
						else
							inheritingFromTmp.Add(_serializedData);

						// Recursion
						inheritedData = _serializedData.inherits;
					}

					// Reverse so we apply in top to bottom order
					inheritingFromTmp.Reverse();

					// Apply
					foreach (var _d in inheritingFromTmp)
						_d.ApplyTo(filenameMappings[_d], instances[d], errors, parameters);
				}

				// Apply data over inherited
				d.ApplyTo(filenameMappings[d], instances[d], errors, parameters);
			}

			// Step 5 - record serialized data in result
			foreach (var kvp in idMapping)
				this.serializedData.Add(kvp.Key, kvp.Value);
		}

		public override string Serialize(List<ISerializableRoot> roots, out List<SerializerError> errors)
		{
			errors = ListPool<SerializerError>.Get();
			XDocument document = new XDocument();
			XElement rootElement = new XElement(parameters.rootElementName);
			document.Add(rootElement);

			foreach (var root in roots)
			{
				var typeName = root.GetType().Name;
				var element = new XElement(typeName);
				element.SetAttributeValue(AttributeIdentifier, root.identifier);
				var data = new SerializedData(SerializerCache.GetSerializableTypeCacheFor(typeName, parameters.standardNamespace), element);
				data.WriteFromObject(root, errors, parameters);
				rootElement.Add(element);
			}

			return document.ToString();
		}
	}

}