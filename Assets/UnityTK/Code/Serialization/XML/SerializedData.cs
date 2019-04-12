using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Serialization.XML
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

		private Dictionary<string, IXmlLineInfo> debug = new Dictionary<string, IXmlLineInfo>();
		private Dictionary<string, CollectionOverrideAction> collectionOverrideActions = new Dictionary<string, CollectionOverrideAction>();
		
		public readonly SerializableTypeCache targetType;
		public readonly XElement xElement;

		/// <summary>
		/// </summary>
		/// <param name="targetType">The target type to be parsed.</param>
		/// <param name="element">The xml element to parse the data from.</param>
		/// <param name="filename">The filename to be reported in case of errors.</param>
		public SerializedData(SerializableTypeCache targetType, XElement element)
		{
			this.targetType = targetType;
			this.xElement = element;

			var inheritsAttrib = element.Attribute(XMLSerializer.AttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
			}
		}

		private List<SerializableTypeCache.FieldCache> _fields = new List<SerializableTypeCache.FieldCache>();
		public void WriteFromObject(object obj, List<ISerializableRoot> referenceables, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			targetType.GetAllFields(_fields);
			foreach (var fieldData in _fields)
			{
				if (!fieldData.isSerialized)
					continue;

				var fieldType = fieldData.serializableTypeCache;
				bool isCollection = SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType);
				IXMLDataSerializer serializer = SerializerCache.GetBestSerializerFor(fieldData.fieldInfo.FieldType);
				string fieldName = fieldData.fieldInfo.Name;

				XElement targetElement = new XElement(fieldName);

				if (!ReferenceEquals(serializer, null))
				{
					try
					{
						if (!SerializerValidation.SerializerWasFound(parameters, targetElement, serializer, fieldName, targetType == null ? null : targetType.type, fieldType == null ? null : fieldType.type, "SERIALIZE", errors))
							continue;
							
						serializer.Serialize(fieldData.fieldInfo.GetValue(obj), targetElement, parameters);
					}
					catch (Exception ex)
					{
						errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, "SERIALIZE", -1, "Serializer threw exception on field " + fieldName + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
					}
				}
				else if (isCollection)
				{
					var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, targetElement);
					col.WriteFromObject(fieldData.fieldInfo.GetValue(obj), targetElement, referenceables, errors, parameters);
				}
				else
				{
					// root reference?
					if (fieldData.isSerializableRoot)
						targetElement.Value = (fieldData.fieldInfo.GetValue(obj) as ISerializableRoot).identifier;
					else
					{
						// Determine which type to serialize
						var targetType = fieldData.serializableTypeCache;
						string typeName = fieldData.fieldInfo.Name;

						// Write class attribute
						targetElement.SetAttributeValue(XMLSerializer.AttributeType, typeName);

						// Field not serializable?
						if (!SerializerValidation.DataFieldSerializerValid(parameters, targetElement, targetType, typeName, fieldName, "SERIALIZE", errors))
							continue;

						// Write field
						var d = new SerializedData(targetType, targetElement);
						d.WriteFromObject(fieldData.fieldInfo.GetValue(obj), referenceables, errors, parameters);
					}
				}

				xElement.Add(targetElement);
			}
		}

		/// <summary>
		/// It will for every field with string data deserialize this data using <see cref="IXMLDataSerializer"/>.
		/// 
		/// For every sub-data field a <see cref="SerializedData"/> object is being written to <see cref="fields"/>.
		/// </summary>
		/// <param name="filename">Only used for error reporting</param>
		public void LoadFieldsFromXML(string filename, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			foreach (var xNode in xElement.Nodes())
			{
				if (!SerializerValidation.NodeIsElement(parameters, xNode, filename, errors)) // Malformed XML
					continue;

				var xElement = xNode as XElement;
				var elementName = xElement.Name.LocalName;

				// Field unknown?
				if (!SerializerValidation.FieldKnown(parameters, xElement, targetType, elementName, filename, errors))
					continue;

				debug.Add(elementName, xElement);
				var _fieldData = targetType.GetFieldData(elementName);
				var fieldData = _fieldData.Value;
				var fieldType = fieldData.serializableTypeCache;
				bool isCollection = SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType);
				IXMLDataSerializer serializer = SerializerCache.GetBestSerializerFor(fieldData.fieldInfo.FieldType);

				if (!ReferenceEquals(serializer, null))
				{
					try
					{
						if (!SerializerValidation.SerializerWasFound(parameters, xElement, serializer, elementName, targetType == null ? null : targetType.type, fieldType == null ? null : fieldType.type, filename, errors))
							continue;

						fields.Add(elementName, serializer.Deserialize(fieldData.fieldInfo.FieldType, xElement, parameters));
					}
					catch (Exception ex)
					{
						errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + elementName + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
					}
				}
				else if (isCollection)
				{
					var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, xElement);
					col.ParseAndLoadData(filename, errors, parameters);
					fields.Add(elementName, col);

					// Collection override action?
					var collectionOverrideAttrib = xElement.Attribute(XMLSerializer.AttributeCollectionOverrideAction);
					if (!ReferenceEquals(collectionOverrideAttrib, null))
						collectionOverrideActions.Set(elementName, (CollectionOverrideAction)Enum.Parse(typeof(CollectionOverrideAction), collectionOverrideAttrib.Value));
				}
				else
				{
					// root reference?
					if (fieldData.isSerializableRoot)
					{
						fields.Add(elementName, new SerializedRootObjectReference()
						{
							identifier = xElement.Value as string
						});
					}
					else
					{
						// Determine which type to serialize
						var targetType = fieldData.serializableTypeCache;
						string typeName = fieldData.fieldInfo.Name;

						// Check if element explicitly overwrites the type to support polymorphism
						// The field type might be some base class type and the xml overwrites this type with a class extending from the base
						var classAttrib = xElement.Attribute(XMLSerializer.AttributeType);
						if (!ReferenceEquals(classAttrib, null))
						{
							targetType = SerializerCache.GetSerializableTypeCacheFor(classAttrib.Value, parameters.standardNamespace);
							typeName = classAttrib.Value;
						}

						// Field not serializable?
						if (!SerializerValidation.DataFieldSerializerValid(parameters, xElement, targetType, typeName, elementName, filename, errors))
							continue;

						// Resolve field name type
						var d = new SerializedData(targetType, xElement as XElement);
						d.LoadFieldsFromXML(filename, errors, parameters);
						fields.Add(elementName, d);
					}
				}
			}
		}

		/// <summary>
		/// The last step for loading data.
		/// In this step, root references are being resolved for this serialized data and their sub data objects.
		/// </summary>
		/// <param name="objects">Root objects to use for remapping</param>
		/// <param name="filename">Only used for error reporting</param>
		public void ResolveReferenceFields(string filename, List<ISerializableRoot> objects, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();

			try
			{
				foreach (var field in fields)
				{
					var @ref = field.Value as SerializedRootObjectReference;
					if (!ReferenceEquals(@ref, null))
						updates.Add(field.Key, @ref.Resolve(objects));

					var sub = field.Value as SerializedData;
					if (!ReferenceEquals(sub, null))
						sub.ResolveReferenceFields(filename, objects, errors, parameters);

					var col = field.Value as SerializedCollectionData;
					if (!ReferenceEquals(col, null))
						col.ResolveReferenceFieldsAndSubData(filename, objects, errors, parameters);
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
		/// <param name="filename">Only used for error reporting</param>
		public void ApplyTo(string filename, object obj, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			foreach (var field in fields)
			{
				var _fieldInfo = this.targetType.GetFieldData(field.Key);
				var fieldInfo = _fieldInfo.Value;
				var value = field.Value;

				var sub = value as SerializedData;
				if (!ReferenceEquals(sub, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);

					// If not, create new obj
					if (ReferenceEquals(value, null))
						value = sub.targetType.Create();

					sub.ApplyTo(filename, value, errors, parameters);
				}

				var col = value as SerializedCollectionData;
				if (!ReferenceEquals(col, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);
					
					CollectionOverrideAction action;
					if (!ReferenceEquals(value, null))
					{
						if (!collectionOverrideActions.TryGetValue(field.Key, out action))
							action = CollectionOverrideAction.Combine; // Always default to comibining

						switch (action)
						{
							case CollectionOverrideAction.Combine: value = col.CombineWithInNew(value); break;
							case CollectionOverrideAction.Replace: value = col.CreateCollection(); break;
						}
					}
					else // Write new collection
						value = col.CreateCollection();
				}

				if (!SerializerValidation.TypeCheck(parameters, debug.TryGet(field.Key), field.Key, value, fieldInfo.fieldInfo.FieldType, filename, errors))
					continue;

				fieldInfo.fieldInfo.SetValue(obj, value);
			}
		}

		/// <summary>
		/// Returns an enumeration to be used to iterate over every reference in this data and all its sub data objects.
		/// </summary>
		public IEnumerable<string> GetReferencedObjectRoots()
		{
			foreach (var field in fields)
			{
				var v = field.Value as SerializedRootObjectReference;
				if (!ReferenceEquals(v, null))
					yield return v.identifier;

				var sub = field.Value as SerializedData;
				if (!ReferenceEquals(sub, null))
					foreach (var @ref in sub.GetReferencedObjectRoots())
						yield return @ref;

				var scd = field.Value as SerializedCollectionData;
				if (!ReferenceEquals(scd, null))
					foreach (var @ref in scd.GetReferencedObjectRoots())
						yield return @ref;
			}
		}
	}
}
