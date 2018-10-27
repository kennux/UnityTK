using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// The main API for UnityTK prototypes.
	/// 
	/// It provides parser methods to parse prototypes from XML.
	/// </summary>
	public static class PrototypeParser
	{
		public const string PrototypeContainerXMLName = "PrototypeContainer";
		public const string PrototypeContainerAttributeType = "Type";

		public const string PrototypeElementXMLName = "Prototype";
		public const string PrototypeAttributeInherits = "Inherits";
		public const string PrototypeAttributeIdentifier = "Id";
		public const string PrototypeAttributeType = "Type";
		public const string PrototypeAttributeAbstract = "Abstract";
		public const string PrototypeAttributeCollectionOverrideAction = "CollectionOverrideAction";

		/// <summary>
		/// Parses the specified XML content and returns all prototypes which could be parsed.
		/// </summary>
		/// <param name="xmlContent">The xml content to use for parsing-</param>
		/// <param name="parameters">The parameters for the parser.</param>
		/// <param name="errors">A list where parsing errors will be written to.</param>
		/// <returns></returns>
		public static List<IPrototype> Parse(string xmlContent, PrototypeParseParameters parameters, ref List<ParsingError> errors)
		{
			PrototypesCaches.LazyInit();
			return _Parse(_PreParse(xmlContent, "DIRECT PARSE", ref parameters, ref errors), ref parameters, ref errors);
		}

		/// <summary>
		/// Same as <see cref="Parse(string, string)"/>, but can parse many xmls with relationships / dependencies in them together.
		/// This will make it possible to have a prototype in one file which is inherited from in another file.
		/// 
		/// The prototypes will be loaded in order and able to resolve references across multiple files!
		/// </summary>
		public static List<IPrototype> Parse(string[] xmlContents, string[] filenames, PrototypeParseParameters parameters, ref List<ParsingError> errors)
		{
			PrototypesCaches.LazyInit();
			List<SerializedData> data = new List<SerializedData>();

			if (xmlContents.Length != filenames.Length)
				throw new ArgumentException("Xml content string count must match filename count in Prototypes.Parse()!");

			for (int i = 0; i < xmlContents.Length; i++)
			{
				_PreParse(xmlContents[i], filenames[i], ref parameters, ref errors, data);
			}

			return _Parse(data, ref parameters, ref errors);
		}
		
		private static List<SerializedData> _PreParse(string xmlContent, string filename, ref PrototypeParseParameters parameters, ref List<ParsingError> errors, List<SerializedData> preAlloc = null)
		{
			ListPool<SerializedData>.GetIfNull(ref preAlloc);
			ListPool<ParsingError>.GetIfNull(ref errors);
			var xElement = XElement.Parse(xmlContent);

			// Validity check
			if (!string.Equals(xElement.Name.LocalName, PrototypeContainerXMLName))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element name '" + xElement.Name + "' is incorrect / not supported, must be '"+PrototypeContainerXMLName+"'! Skipping file!"));
				return preAlloc;
			}
			if (!xElement.HasAttributes)
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element has no attributes! Need atleast '"+PrototypeContainerAttributeType+"' attribute specifying the type of the prototypes to be loaded! Skipping container!"));
				return preAlloc;
			}

			// Get type
			XAttribute typeAttribute = xElement.Attribute(PrototypeContainerAttributeType);
			if (ReferenceEquals(typeAttribute, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element missing '"+PrototypeContainerAttributeType+"'! Need '"+PrototypeContainerAttributeType+"' attribute specifying the type of the prototypes to be loaded! Skipping container!"));
				return preAlloc;
			}

			var type = LookupSerializableTypeCache(typeAttribute.Value, ref parameters);
			if (ReferenceEquals(type, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element type " + typeAttribute.Value + " unknown! Skipping file!"));
				return preAlloc;
			}

			// Iterate over nodes
			foreach (var node in xElement.Nodes())
			{
				var elementType = type;

				// Validity checks
				if (!(node is XElement)) // Malformed XML
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}

				XElement nodeElement = node as XElement;
				if (!string.Equals(nodeElement.Name.LocalName, PrototypeElementXMLName)) // Unsupported
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (nodeElement as IXmlLineInfo).LineNumber, "Element name '" + nodeElement.Name + "' is incorrect / not supported, must be '"+PrototypeElementXMLName+"'! Skipping element!"));
					return preAlloc;
				}

				var elementTypeAttribute = nodeElement.Attribute(PrototypeContainerAttributeType);
				if (!ReferenceEquals(elementTypeAttribute, null))
				{
					elementType = LookupSerializableTypeCache(elementTypeAttribute.Value, ref parameters);
					if (ReferenceEquals(elementType, null))
					{
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (nodeElement as IXmlLineInfo).LineNumber, "Element type " + elementTypeAttribute.Value + " unknown! Skipping file!"));
						return preAlloc;
					}
				}

				// Prepare
				var data = new SerializedData(elementType, nodeElement, filename);
				preAlloc.Add(data);
			}
			
			return preAlloc;
		}

		private static List<IPrototype> _Parse(List<SerializedData> data, ref PrototypeParseParameters parameters, ref List<ParsingError> errors, List<IPrototype> preAlloc = null)
		{
			ListPool<IPrototype>.GetIfNull(ref preAlloc);
			ListPool<ParsingError>.GetIfNull(ref errors);

			// Get prototypes with others inheriting from first

			// Key = type which is inheriting from something, Value = the type its inheriting from
			Dictionary<SerializedData, List<SerializedData>> inheritingFrom = new Dictionary<SerializedData, List<SerializedData>>();
			Dictionary<SerializedData, object> instances = new Dictionary<SerializedData, object>();
			Dictionary<string, SerializedData> nameMapping = new Dictionary<string, SerializedData>();
			List<SerializedData> invalid = new List<SerializedData>();

			// Pre-parse names, create instances and apply name
			foreach (var d in data)
			{
				// Read name
				var attribName = d.element.Attribute(PrototypeAttributeIdentifier);
				if (ReferenceEquals(attribName, null))
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, d.filename, (d.element as IXmlLineInfo).LineNumber, "Prototype without identifier! Skipping prototype!"));
					invalid.Add(d);
					continue;
				}

				nameMapping.Add(attribName.Value, d);

				// Check if abstract prototype data
				var attribAbstract = d.element.Attribute(PrototypeAttributeAbstract);
				bool isAbstract = !ReferenceEquals(attribAbstract, null) && string.Equals("True", attribAbstract.Value);
				
				if (!isAbstract)
				{
					var obj = d.targetType.Create();
					instances.Add(d, obj);

					(obj as IPrototype).identifier = attribName.Value;
					preAlloc.Add(obj as IPrototype);
				}
			}
			
			// Remove invalidated entries
			foreach (var d in invalid)
				data.Remove(d);

			invalid.Clear();
			foreach (var d in data)
			{
				if (!string.IsNullOrEmpty(d.inherits))
				{
					SerializedData inheritedData = nameMapping[d.inherits];

					if (ReferenceEquals(inheritedData, null))
					{
						// TODO: Line number
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, d.filename, -1, "Prototype is inheriting from unknown prototype '" + d.inherits + "'! Skipping prototype!"));
						invalid.Add(d);
						continue;
					}
					else
					{
						inheritingFrom.GetOrCreate(d).Add(inheritedData);
					}
				}
			}

			PrototypeParserState state = new PrototypeParserState()
			{
				parameters = parameters
			};

			// Remove invalidated entries
			foreach (var d in invalid)
				data.Remove(d);

			// Step 1 - sort by inheritance
			List<SerializedData> empty = new List<SerializedData>();
			var sorted = data.TSort((sd) => inheritingFrom.ContainsKey(sd) ? inheritingFrom[sd] : empty, true).ToList();

			// Step 2 - Preloads the fields and creates sub-data objects
			foreach (var d in sorted)
				d.LoadFields(errors, state);

			// Step 3 - run sorting algorithm for reference resolve
			foreach (var d in data)
				d.ResolveReferenceFields(preAlloc, errors, state);

			// Step 4 - Final data apply
			foreach (var d in sorted)
			{
				if (!instances.ContainsKey(d))
					continue;

				// Apply inherited data first
				if (!string.IsNullOrEmpty(d.inherits))
					nameMapping[d.inherits].ApplyTo(instances[d], errors, state);

				// Apply data over inherited
				d.ApplyTo(instances[d], errors, state);
			}

			return preAlloc;
		}

		private static SerializableTypeCache LookupSerializableTypeCache(string name, ref PrototypeParseParameters parameters)
		{
			return PrototypesCaches.LookupSerializableTypeCache(name, parameters.standardNamespace);
		}
	}
}