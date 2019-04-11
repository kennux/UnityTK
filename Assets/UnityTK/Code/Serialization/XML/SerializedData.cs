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
		public readonly string filename;

		/// <summary>
		/// </summary>
		/// <param name="targetType">The target type to be parsed.</param>
		/// <param name="element">The xml element to parse the data from.</param>
		/// <param name="filename">The filename to be reported in case of errors.</param>
		public SerializedData(SerializableTypeCache targetType, XElement element, string filename)
		{
			this.targetType = targetType;
			this.xElement = element;
			this.filename = filename;

			var inheritsAttrib = element.Attribute(XMLSerializer.AttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
			}
		}

		/// <summary>
		/// It will for every field with string data deserialize this data using <see cref="IXMLDataSerializer"/>.
		/// 
		/// For every sub-data field a <see cref="SerializedData"/> object is being written to <see cref="fields"/>.
		/// </summary>
		public void LoadFields(List<ParsingError> errors, XMLSerializerParams parameters)
		{
			foreach (var xNode in xElement.Nodes())
			{
				if (!ParsingValidation.NodeIsElement(parameters, xNode, this.filename, errors)) // Malformed XML
					continue;

				var xElement = xNode as XElement;
				var elementName = xElement.Name.LocalName;

				// Field unknown?
				if (!ParsingValidation.FieldKnown(parameters, xElement, targetType, elementName, filename, errors))
					continue;

				debug.Add(elementName, xElement);
				var fieldData = targetType.GetFieldData(elementName);
				var fieldType = fieldData.serializableTypeCache;
				bool isCollection = SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType);
				IXMLDataSerializer serializer = SerializerCache.GetBestSerializerFor(fieldData.fieldInfo.FieldType);

				if (!ReferenceEquals(serializer, null))
				{
					try
					{
						if (!ParsingValidation.SerializerWasFound(parameters, xElement, serializer, elementName, targetType == null ? null : targetType.type, fieldType == null ? null : fieldType.type, filename, errors))
							continue;

						fields.Add(elementName, serializer.Deserialize(fieldData.fieldInfo.FieldType, xElement, parameters));
					}
					catch (Exception ex)
					{
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + elementName + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
					}
				}
				else if (isCollection)
				{
					var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, xElement, this.filename);
					col.ParseAndLoadData(errors, parameters);
					fields.Add(elementName, col);

					// Collection override action?
					var collectionOverrideAttrib = xElement.Attribute(XMLSerializer.AttributeCollectionOverrideAction);
					if (!ReferenceEquals(collectionOverrideAttrib, null))
						collectionOverrideActions.Set(elementName, (CollectionOverrideAction)Enum.Parse(typeof(CollectionOverrideAction), collectionOverrideAttrib.Value));
				}
				else
				{
					// A known serializable

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
						if (!ParsingValidation.DataFieldSerializerValid(parameters, xElement, targetType, typeName, elementName, filename, errors))
							continue;

						// Resolve field name type
						var d = new SerializedData(targetType, xElement as XElement, this.filename);
						d.LoadFields(errors, parameters);
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
		/// <param name="errors"></param>
		public void ResolveReferenceFields(List<ISerializableRoot> objects, List<ParsingError> errors, XMLSerializerParams parameters)
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
						sub.ResolveReferenceFields(objects, errors, parameters);

					var col = field.Value as SerializedCollectionData;
					if (!ReferenceEquals(col, null))
						col.ResolveReferenceFieldsAndSubData(objects, errors, parameters);
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
		public void ApplyTo(object obj, List<ParsingError> errors, XMLSerializerParams parameters)
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

					sub.ApplyTo(value, errors, parameters);
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

				if (!ParsingValidation.TypeCheck(parameters, debug.TryGet(field.Key), field.Key, value, fieldInfo.fieldInfo.FieldType, filename, errors))
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
