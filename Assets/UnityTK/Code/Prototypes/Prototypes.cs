using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Prototypes
{
	public static class Prototypes
	{
		public const string PrototypeContainerXMLName = "PrototypeContainer";
		public const string PrototypeContainerAttributeType = "Type";

		public const string PrototypeElementXMLName = "Prototype";
		public const string PrototypeAttributeInherits = "Inherits";
		public const string PrototypeAttributeName = "Name";

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
				_PreParse(xmlContents[i], filenames[i], ref parameters, ref errors);
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

			var type = ResolveType(typeAttribute.Value, ref parameters);
			if (ReferenceEquals(type, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element type " + typeAttribute.Value + " unknown! Skipping file!"));
				return preAlloc;
			}

			// Iterate over nodes
			foreach (var node in xElement.Nodes())
			{
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

				XAttribute nameAttrib = nodeElement.Attribute(PrototypeAttributeName);
				if (ReferenceEquals(nameAttrib, null)) // Require name attrib
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (nodeElement as IXmlLineInfo).LineNumber, "Element has no name! Elements must have a '" + PrototypeAttributeName + "' attribute! Skipping element!"));
					continue;
				}

				// Prepare
				var data = new SerializedData();
				data.PrepareParse(type, nodeElement, filename);
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
			List<SerializedData> removeData = new List<SerializedData>();
			foreach (var d in data)
			{
				if (!string.IsNullOrEmpty(d.inherits))
				{
					SerializedData inheritedData = LookupData(data, d.inherits);

					if (ReferenceEquals(inheritedData, null))
					{
						// TODO: Line number
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, d.filename, -1, "Prototype '" + d.name + "' is inheriting from unknown prototype '" + d.inherits + "'! Skipping prototype!"));
						continue;
					}
					else
					{
						inheritingFrom.GetOrCreate(d).Add(inheritedData);
					}
				}
			}

			// Remove invalidated entries
			foreach (var d in removeData)
				data.Remove(d);

			// Step 1 - sort by inheritance
			List<SerializedData> empty = new List<SerializedData>();
			var sorted = data.TSort((sd) => inheritingFrom.ContainsKey(sd) ? inheritingFrom[sd] : empty, true).ToList();
			
			// Step 2 - parse fields
			foreach (var d in sorted)
				d.ParseFields(errors);

			// Step 3 - Preloads the fields and creates sub-data objects
			foreach (var d in sorted)
				d.PreLoadFields(errors);

			// Step 4 - create prototypes
			foreach (var d in sorted)
				instances.Add(d, d.Create());

			// Step 5 - run sorting algorithm for reference resolve
			sorted = data.TSort((sd) => sd.GetReferencedPrototypes().Select((r) => LookupData(data, r)), true).ToList();
			foreach (var d in sorted)
				d.ResolveReferenceFieldsAndSubData(instances[d], preAlloc, errors);

			// Step 6 - Final data apply
			foreach (var d in sorted)
				d.Apply(instances[d], errors);

			foreach (var i in instances)
				preAlloc.Add(i.Value as IPrototype);

			return preAlloc;
		}

		private static SerializedData LookupData(List<SerializedData> data, string lookupName)
		{
			foreach (var d in data)
			{
				if (string.Equals(d.name, lookupName))
					return d;
			}

			return null;
		}

		private static SerializableTypeCache ResolveType(string name, ref PrototypeParseParameters parameters)
		{
			return PrototypesCaches.LookupTypeCache(name, parameters.standardNamespace);
		}
	}
}