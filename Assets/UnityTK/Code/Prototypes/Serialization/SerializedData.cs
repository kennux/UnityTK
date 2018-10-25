using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Prototypes
{
	class SerializedData
	{
		public string inherits;
		
		/// <summary>
		/// All fields serialized in the data of this object.
		/// </summary>
		private Dictionary<string, object> fields = new Dictionary<string, object>();

		/// <summary>
		/// All subinstances referenced in <see cref="fields"/> mapped to their instances.
		/// </summary>
		private Dictionary<SerializedData, object> subInstances = new Dictionary<SerializedData, object>();

		class PrototypeReference
		{
			public string name;

			public IPrototype Resolve(List<IPrototype> prototypes)
			{
				foreach (var p in prototypes)
				{
					if (string.Equals(p.name, this.name))
						return p;
				}

				return null;
			}
		}

		public readonly SerializableTypeCache targetType;
		public readonly XElement element;
		public readonly string filename;

		/// <summary>
		/// </summary>
		/// <param name="targetType">The target type to be parsed.</param>
		/// <param name="element">The xml element to parse the data from.</param>
		/// <param name="filename">The filename to be reported in case of errors.</param>
		public SerializedData(SerializableTypeCache targetType, XElement element, string filename)
		{
			this.targetType = targetType;
			this.element = element;
			this.filename = filename;

			var inheritsAttrib = element.Attribute(Prototypes.PrototypeAttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
			}
		}

		/// <summary>
		/// Parses field data from the XMLElement element.
		/// This will iterate over every node in the xml element. For every node, an entry in <see cref="fields"/> is being created.
		/// The entry may be of type string for nodes without sub-nodes (ex. <test>123</test> -> field name = test - field value = 123).
		/// The entry also may be an xml node used to create sub data.
		/// </summary>
		public void ParseFields(List<ParsingError> errors, PrototypeParserState state)
		{
			foreach (var node in element.Nodes())
			{
				if (!(node is XElement)) // Malformed XML
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}
				var nodeElement = node as XElement;

				// Parse element recursively
				if (nodeElement.HasElements)
					fields.Set(nodeElement.Name.LocalName, nodeElement);
				else
					fields.Set(nodeElement.Name.LocalName, nodeElement.Value);
			}
		}

		/// <summary>
		/// Actually loads the data from the previous parse <see cref="ParseFields(List{ParsingError})"/>.
		/// It will for every field with string data deserialize this data using <see cref="IPrototypeSerializer"/>.
		/// 
		/// For every sub-data field (<seealso cref="ParseFields(List{ParsingError})"/>) a <see cref="SerializedData"/> object is being written to <see cref="fields"/>.
		/// The sub-data object will have <see cref="PrepareParse(SerializableTypeCache, XElement, string)"/>, <see cref="ParseFields(List{ParsingError})"/> and <see cref="LoadFields(List{ParsingError}, PrototypeParserState)"/> called.
		/// </summary>
		public void LoadFields(List<ParsingError> errors, PrototypeParserState state)
		{
			Dictionary<string, object> updates = new Dictionary<string, object>();
			List<string> removeFields = new List<string>();
			
			// First, resolve elements left in the fields dictionary
			foreach (var field in fields)
			{
				// Field unknown?
				if (!targetType.HasField(field.Key))
				{
					// TODO: Line number
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Unknown field " + field.Key + "! Skipping field!"));
					removeFields.Add(field.Key);
					continue;
				}

				var fieldData = targetType.GetFieldData(field.Key);
				var element = field.Value;
				if (element is XElement)
				{
					var xElement = element as XElement;

					// Is this field a collection?
					if (SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType))
					{
						var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, xElement, this.filename);
						col.ParseAndLoadData(errors, state);
						updates.Add(field.Key, col);
					}
					else // Not a collection, but a sub-data element
					{
						// Determine which type to serialize
						var serializableTypeCache = fieldData.serializableTypeCache;
						string typeName = fieldData.fieldInfo.Name;

						// Check if element explicitly overwrites the type to support polymorphism
						// The field type might be some base class type and the xml overwrites this type with a class extending from the base
						var classAttrib = xElement.Attribute(Prototypes.PrototypeAttributeClass);
						if (!ReferenceEquals(classAttrib, null))
						{
							serializableTypeCache = PrototypesCaches.LookupSerializableTypeCache(classAttrib.Value, state.parameters.standardNamespace);
							typeName = classAttrib.Value;
						}

						// Field not serializable?
						if (ReferenceEquals(serializableTypeCache, null))
						{
							// TODO: Line number
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Field with unknown type " + typeName + " - unknown by the serializer cache! Are you missing " + nameof(PrototypesTypeSerializableAttribute) + " attribute? Skipping field!"));
							removeFields.Add(field.Key);
							continue;
						}

						// Resolve field name type
						var d = new SerializedData(serializableTypeCache, element as XElement, this.filename);
						d.ParseFields(errors, state);
						d.LoadFields(errors, state);
						subInstances.Add(d, d.targetType.Create());
						updates.Add(field.Key, d);
					}
				}
				else if (element is string)
				{
					if (fieldData.isPrototype)
					{
						// This is a reference!
						updates.Add(field.Key, new PrototypeReference()
						{
							name = element as string
						});
					}
					else
					{
						try
						{
							var serializer = PrototypesCaches.GetBestSerializerFor(fieldData.fieldInfo.FieldType);
							if (ReferenceEquals(serializer, null))
							{
								errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer for field " + field.Key + " on type " + targetType.type + " (" + fieldData.fieldInfo.FieldType + ") could not be found! Skipping field!"));
								removeFields.Add(field.Key);
								continue;
							}

							updates.Add(field.Key, serializer.Deserialize(fieldData.fieldInfo.FieldType, field.Value as string, state));
						}
						catch (Exception ex)
						{
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + field.Key + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
							removeFields.Add(field.Key);
						}
					}
				}
			}

			// Write updates
			foreach (var update in updates)
				this.fields[update.Key] = update.Value;

			// Remove fields
			foreach (var field in removeFields)
				this.fields.Remove(field);
		}

		/// <summary>
		/// The last step for loading data.
		/// In this step, prototype references are being resolved for this serialized data and their sub data objects.
		/// </summary>
		/// <param name="prototypes">Prototypes to use for remapping</param>
		/// <param name="errors"></param>
		public void ResolveReferenceFieldsAndSubData(List<IPrototype> prototypes, List<ParsingError> errors, PrototypeParserState state)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();

			try
			{
				foreach (var field in fields)
				{
					var @ref = field.Value as PrototypeReference;
					if (!ReferenceEquals(@ref, null))
					{
						updates.Add(field.Key, @ref.Resolve(prototypes));
					}

					var sub = field.Value as SerializedData;
					if (!ReferenceEquals(sub, null))
					{
						sub.ResolveReferenceFieldsAndSubData(prototypes, errors, state);
						sub.ApplyTo(subInstances[sub], errors, state);
						updates.Add(field.Key, subInstances[sub]);
					}

					var col = field.Value as SerializedCollectionData;
					if (!ReferenceEquals(col, null))
					{
						updates.Add(field.Key, col.GetCollectionResolveReferenceFieldsAndSubData(prototypes, errors, state));
					}
				}
				
				// Write updates
				foreach (var update in updates)
					this.fields[update.Key] = update.Value;
			}
			finally
			{
				DictionaryPool<string, object>.Return(updates);
			}
		}

		/// <summary>
		/// Applies the data stored in this serialized data to the specified object using reflection.
		/// </summary>
		public void ApplyTo(object obj, List<ParsingError> errors, PrototypeParserState state)
		{
			foreach (var field in fields)
			{
				var fieldInfo = this.targetType.GetFieldData(field.Key);
				if (!ReferenceEquals(field.Value, null) && !fieldInfo.fieldInfo.FieldType.IsAssignableFrom(field.Value.GetType()))
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Fatal error deserializing field " + field.Key + " - tried applying field data but types mismatched! Stored type: " + field.Value.GetType() + " - Declared type: " + fieldInfo.fieldInfo.FieldType + "! Skipping field!"));
					continue;
				}

				fieldInfo.fieldInfo.SetValue(obj, field.Value);
			}
		}

		/// <summary>
		/// Returns an enumeration to be used to iterate over every reference in this data and all its sub data objects.
		/// </summary>
		public IEnumerable<string> GetReferencedPrototypes()
		{
			foreach (var field in fields)
			{
				var v = field.Value as PrototypeReference;
				if (!ReferenceEquals(v, null))
					yield return v.name;

				var sub = field.Value as SerializedData;
				if (!ReferenceEquals(sub, null))
					foreach (var @ref in sub.GetReferencedPrototypes())
						yield return @ref;
			}
		}
	}
}
