using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Serialization.XML
{
	/// <summary>
	/// <see cref="SerializedData"/> alike object for storing collections.
	/// </summary>
	class SerializedCollectionData
	{
		public static bool IsCollection(Type type)
		{
			return typeof(ICollection).IsAssignableFrom(type) || type.GetInterfaces().Any(x =>
				  x.IsGenericType &&
				  x.GetGenericTypeDefinition() == typeof(ISet<>));
		}

		/// <summary>
		/// Reads the element type from the specified collection type.
		/// </summary>
		public Type GetElementType(Type collectionType)
		{
			if (collectionType.IsArray)
				return collectionType.GetElementType();
			else
			{
				// Generic collection?
				var collection = collectionType.GetInterfaces().FirstOrDefault(x =>
				  x.IsGenericType &&
				  x.GetGenericTypeDefinition() == typeof(ICollection<>));

				if (!ReferenceEquals(collection, null))
					return collection.GetGenericArguments()[0];

				// Generic set?
				var set = collectionType.GetInterfaces().FirstOrDefault(x =>
				  x.IsGenericType &&
				  x.GetGenericTypeDefinition() == typeof(ISet<>));

				if (!ReferenceEquals(set, null))
					return set.GetGenericArguments()[0];
			}

			return typeof(object);
		}

		/// <summary>
		/// Creates the instance of a collection.
		/// Either a C# collection or an array is supported.
		/// </summary>
		public static object GetCollectionInstance(Type collectionType, int length)
		{
			if (collectionType.IsArray)
				return Array.CreateInstance(collectionType.GetElementType(), length);
			else
				return Activator.CreateInstance(collectionType);
		}

		/// <summary>
		/// Tries to write the specified element into the specified collection at the specified index.
		/// First, collection will be casted to IList. If that worked, the element is just added.
		/// 
		/// If not, it will try to reflect an "Add" method from the collection type.
		/// If it found one, it will call it with the element - ignoring the index.
		/// 
		/// Index can be set to -1 to just add the element somewhere (preferrably at the end).
		/// </summary>
		private static void WriteElementToCollection(object collection, object element, int index = -1)
		{
			IList list = collection as IList;
			if (!ReferenceEquals(list, null))
			{
				int count = list.Count;
				if (index == -1 || count <= index)
					list.Add(element);
				else
					list[index] = element;
				return;
			}

			// Well... Let's try our best!
			var addMethod = collection.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");
			if (ReferenceEquals(addMethod, null))
				throw new System.NotImplementedException("Unknown collection type " + collection.GetType() + "! XML Serializer cannot write data to this collection!");

			addMethod.Invoke(collection, new object[] { element });
		}

		/// <summary>
		/// All elements of this collection.
		/// </summary>
		private List<object> elements = new List<object>();

		public readonly Type collectionType;
		public readonly XElement xElement;
		
		/// <summary>
		/// </summary>
		/// <param name="collectionType">The collection type to be parsed.</param>
		/// <param name="xElement">The xml element to parse the data from.</param>
		public SerializedCollectionData(Type collectionType, XElement xElement)
		{
			this.collectionType = collectionType;
			this.xElement = xElement;
		}
		
		/// <summary>
		/// Serializes the collection obj to <see cref="xElement"/>.
		/// </summary>
		/// <param name="targetElement">The element to write the loaded data to.</param>
		/// <param name="obj">The collection to load and write to <see cref="xElement"/></param>
		/// <param name="referenceables">Serializable object roots which will possibly be referenced by elements in obj.</param>
		public void WriteFromObject(object collection, XElement targetElement, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			Type elementType = GetElementType(this.collectionType);
			var elementTypeCache = SerializerCache.GetSerializableTypeCacheFor(elementType);
			string elementTypeName = elementType.Name;

			foreach (var obj in (collection as IEnumerable))
			{
				XElement _targetElement = new XElement("li");

				if (typeof(ISerializableRoot).IsAssignableFrom(elementType))
				{
					// object ref
					this.elements.Add((obj as ISerializableRoot).identifier);
				}
				else
				{
					// Determine type
					var serializableTypeCache = elementTypeCache;
					string typeName = elementTypeName;

					// Write class attribute
					_targetElement.SetAttributeValue(XMLSerializer.AttributeType, typeName);

					// Validity checks
					// Field not serializable?
					if (ReferenceEquals(serializableTypeCache, null))
					{
						errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, "SERIALIZER", -1, "Collection element with unknown type " + typeName + " unserializable! Skipping field!"));
						continue;
					}

					// Add element
					new SerializedData(serializableTypeCache, _targetElement).WriteFromObject(obj, errors, parameters);
				}

				targetElement.Add(_targetElement);
			}
		}

		/// <summary>
		/// <see cref="SerializedData.LoadFields(List{SerializerError}, PrototypeParserState)"/>
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="filename">The filename from where data will be parsed. Only used for error reporting.</param>
		public void ParseAndLoadData(string filename, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			var elementNodes = xElement.Nodes().ToList();
			var collection = GetCollectionInstance(this.collectionType, elementNodes.Count);

			Type elementType = GetElementType(this.collectionType);
			var elementTypeCache = SerializerCache.GetSerializableTypeCacheFor(elementType);
			string elementTypeName = elementType.Name;

			foreach (var node in elementNodes)
			{
				var xElementNode = node as XElement;
				if (ReferenceEquals(xElementNode, null)) // Malformed XML
				{
					errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}

				if (typeof(ISerializableRoot).IsAssignableFrom(elementType))
				{
					// object ref
					this.elements.Add(new SerializedRootObjectReference()
					{
						identifier = xElementNode.Value
					});
				}
				else
				{
					// Determine type
					var serializableTypeCache = elementTypeCache;
					string typeName = elementTypeName;

					// Try to read class attrib
					var classAttrib = xElementNode.Attribute(XMLSerializer.AttributeType);
					if (!ReferenceEquals(classAttrib, null))
					{
						serializableTypeCache = SerializerCache.GetSerializableTypeCacheFor(classAttrib.Value, parameters.standardNamespace);
						typeName = classAttrib.Value;
					}

					// Validity checks
					// Field not serializable?
					if (ReferenceEquals(serializableTypeCache, null))
					{
						// TODO: Line number, better reporting as to why this type is unserializable!
						errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, -1, "Collection element with unknown type " + typeName + " unserializable! Skipping field!"));
						continue;
					}

					// Add element
					this.elements.Add(new SerializedData(serializableTypeCache, xElementNode));
				}
			}

			foreach (var element in this.elements)
			{
				var sElement = element as SerializedData;
				if (!ReferenceEquals(sElement, null))
				{
					sElement.LoadFieldsFromXML(filename, errors, parameters);
				}
			}
		}

		public IEnumerable<string> GetReferencedObjectRoots()
		{
			foreach (var element in this.elements)
			{
				var sd = element as SerializedData;
				if (!ReferenceEquals(sd, null))
					foreach (var @ref in sd.GetReferencedObjectRoots())
						yield return @ref;
			}
		}
		
		public void ResolveReferenceFieldsAndSubData(string filename, List<ISerializableRoot> objects, List<SerializerError> errors, XMLSerializerParams parameters)
		{
			for (int i = 0; i < this.elements.Count; i++)
			{
				// Finalize, create and apply
				var element = this.elements[i];
				var sElement = element as SerializedData;
				var protoRef = element as SerializedRootObjectReference;
				
				if (!ReferenceEquals(sElement, null))
				{
					sElement.ResolveReferenceFields(filename, objects, errors, parameters);
					var value = sElement.targetType.Create();
					sElement.ApplyTo(filename, value, errors, parameters);
					this.elements[i] = value;
				}
				else if (!ReferenceEquals(protoRef, null))
					this.elements[i] = protoRef.Resolve(objects);

			}
		}

		public object CombineWithInNew(object otherCollection)
		{
			var otherCount = (otherCollection as IEnumerable).Cast<object>().Count();
			int c = this.elements.Count + otherCount, i = 0;

			var collection = GetCollectionInstance(this.collectionType, c);
			foreach (var obj in (otherCollection as IEnumerable).Cast<object>())
			{
				WriteElementToCollection(collection, obj, i);
				i++;
			}

			for (int j = 0; j < this.elements.Count; j++)
			{
				WriteElementToCollection(collection, this.elements[j], i + j);
			}

			return collection;
		}

		public object CreateCollection()
		{
			var collection = GetCollectionInstance(this.collectionType, this.elements.Count);
			for (int i = 0; i < this.elements.Count; i++)
			{
				// Write to collection
				WriteElementToCollection(collection, this.elements[i], i);
			}
			return collection;
		}
	}
}