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
		public enum CollectionOverrideAction
		{
			Replace,
			Combine
		}

		public string inherits;
		
		/// <summary>
		/// All fields serialized in the data of this object.
		/// </summary>
		private Dictionary<string, object> fields = new Dictionary<string, object>();

		private Dictionary<string, CollectionOverrideAction> collectionOverrideActions = new Dictionary<string, CollectionOverrideAction>();
		
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

			var inheritsAttrib = element.Attribute(PrototypeParser.PrototypeAttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
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
			foreach (var node in element.Nodes())
			{
				if (!(node is XElement)) // Malformed XML
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}
				var xElement = node as XElement;
				var elementName = xElement.Name.LocalName;

				// Field unknown?
				if (!targetType.HasField(elementName))
				{
					// TODO: Line number
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Unknown field " + elementName + "! Skipping field!"));
					continue;
				}
				
				var fieldData = targetType.GetFieldData(elementName);
				var fieldType = fieldData.serializableTypeCache;
				if (fieldType == null)
				{
					// Not a prototype serializable

					// Is this field a collection?
					if (SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType))
					{
						var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, xElement, this.filename);
						col.ParseAndLoadData(errors, state);
						fields.Add(elementName, col);

						// Collection override action?
						var collectionOverrideAttrib = xElement.Attribute(PrototypeParser.PrototypeAttributeCollectionOverrideAction);
						if (!ReferenceEquals(collectionOverrideAttrib, null))
							collectionOverrideActions.Set(elementName, (CollectionOverrideAction)Enum.Parse(typeof(CollectionOverrideAction), collectionOverrideAttrib.Value));
					}
					// Value type serialized
					else
					{
						try
						{
							var serializer = PrototypesCaches.GetBestSerializerFor(fieldData.fieldInfo.FieldType);
							if (ReferenceEquals(serializer, null))
							{
								errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer for field " + elementName + " on type " + targetType.type + " (" + fieldData.fieldInfo.FieldType + ") could not be found! Skipping field!"));
								continue;
							}

							fields.Add(elementName, serializer.Deserialize(fieldData.fieldInfo.FieldType, xElement, state));
						}
						catch (Exception ex)
						{
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + elementName + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
							continue;
						}
					}

				}
				else
				{
					// A known serializable

					// Prototype reference?
					if (fieldData.isPrototype)
					{
						fields.Add(elementName, new SerializedPrototypeReference()
						{
							name = xElement.Value as string
						});
					}
					else
					{
						// Determine which type to serialize
						var serializableTypeCache = fieldData.serializableTypeCache;
						string typeName = fieldData.fieldInfo.Name;

						// Check if element explicitly overwrites the type to support polymorphism
						// The field type might be some base class type and the xml overwrites this type with a class extending from the base
						var classAttrib = xElement.Attribute(PrototypeParser.PrototypeAttributeType);
						if (!ReferenceEquals(classAttrib, null))
						{
							serializableTypeCache = PrototypesCaches.LookupSerializableTypeCache(classAttrib.Value, state.parameters.standardNamespace);
							typeName = classAttrib.Value;
						}

						// Field not serializable?
						if (ReferenceEquals(serializableTypeCache, null))
						{
							// TODO: Line number
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Field '" + elementName + "' with unknown type " + typeName + " - unknown by the serializer cache! Are you missing " + nameof(PrototypesTypeSerializableAttribute) + " attribute? Skipping field!"));
							continue;
						}

						// Resolve field name type
						var d = new SerializedData(serializableTypeCache, xElement as XElement, this.filename);
						d.LoadFields(errors, state);
						fields.Add(elementName, d);
					}
				}
			}
		}

		/// <summary>
		/// The last step for loading data.
		/// In this step, prototype references are being resolved for this serialized data and their sub data objects.
		/// </summary>
		/// <param name="prototypes">Prototypes to use for remapping</param>
		/// <param name="errors"></param>
		public void ResolveReferenceFields(List<IPrototype> prototypes, List<ParsingError> errors, PrototypeParserState state)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();

			try
			{
				foreach (var field in fields)
				{
					var @ref = field.Value as SerializedPrototypeReference;
					if (!ReferenceEquals(@ref, null))
						updates.Add(field.Key, @ref.Resolve(prototypes));

					var sub = field.Value as SerializedData;
					if (!ReferenceEquals(sub, null))
						sub.ResolveReferenceFields(prototypes, errors, state);

					var col = field.Value as SerializedCollectionData;
					if (!ReferenceEquals(col, null))
						col.ResolveReferenceFieldsAndSubData(prototypes, errors, state);
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
				var value = field.Value;

				var sub = value as SerializedData;
				if (!ReferenceEquals(sub, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);

					// If not, create new obj
					if (ReferenceEquals(value, null))
						value = sub.targetType.Create();

					sub.ApplyTo(value, errors, state);
				}

				var col = value as SerializedCollectionData;
				if (!ReferenceEquals(col, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);
					
					CollectionOverrideAction action;
					if (!ReferenceEquals(value, null) && collectionOverrideActions.TryGetValue(field.Key, out action))
					{
						switch (action)
						{
							case CollectionOverrideAction.Combine: value = col.CombineWithInNew(value); break;
							case CollectionOverrideAction.Replace: value = col.CreateCollection(); break;
						}
					}
					else // Write new collection
						value = col.CreateCollection();
				}

				if (!ReferenceEquals(value, null) && !fieldInfo.fieldInfo.FieldType.IsAssignableFrom(value.GetType()))
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Fatal error deserializing field " + field.Key + " - tried applying field data but types mismatched! Stored type: " + value.GetType() + " - Declared type: " + fieldInfo.fieldInfo.FieldType + "! Skipping field!"));
					continue;
				}

				fieldInfo.fieldInfo.SetValue(obj, value);
			}
		}

		/// <summary>
		/// Returns an enumeration to be used to iterate over every reference in this data and all its sub data objects.
		/// </summary>
		public IEnumerable<string> GetReferencedPrototypes()
		{
			foreach (var field in fields)
			{
				var v = field.Value as SerializedPrototypeReference;
				if (!ReferenceEquals(v, null))
					yield return v.name;

				var sub = field.Value as SerializedData;
				if (!ReferenceEquals(sub, null))
					foreach (var @ref in sub.GetReferencedPrototypes())
						yield return @ref;

				var scd = field.Value as SerializedCollectionData;
				if (!ReferenceEquals(scd, null))
					foreach (var @ref in scd.GetReferencedPrototypes())
						yield return @ref;
			}
		}
	}
}
