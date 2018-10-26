using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Prototypes
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
		/// </summary>
		private static void WriteElementToCollection(object collection, object element, int index)
		{
			IList list = collection as IList;
			if (!ReferenceEquals(list, null))
			{
				int count = list.Count;
				if (count <= index)
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
		public readonly string filename;
		
		/// <summary>
		/// </summary>
		/// <param name="collectionType">The collection type to be parsed.</param>
		/// <param name="element">The xml element to parse the data from.</param>
		/// <param name="filename">The filename to be reported in case of errors.</param>
		public SerializedCollectionData(Type collectionType, XElement xElement, string filename)
		{
			this.collectionType = collectionType;
			this.xElement = xElement;
			this.filename = filename;
		}

		/// <summary>
		/// <see cref="SerializedData.ParseFields(List{ParsingError}, PrototypeParserState)"/>
		/// <see cref="SerializedData.LoadFields(List{ParsingError}, PrototypeParserState)"/>
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="state"></param>
		public void ParseAndLoadData(List<ParsingError> errors, PrototypeParserState state)
		{
			var elementNodes = xElement.Nodes().ToList();
			var collection = GetCollectionInstance(this.collectionType, elementNodes.Count);

			Type elementType = GetElementType(this.collectionType);
			var elementTypeCache = PrototypesCaches.GetSerializableTypeCacheFor(elementType);
			string elementTypeName = elementType.Name;

			foreach (var node in elementNodes)
			{
				var xElementNode = node as XElement;
				if (ReferenceEquals(xElementNode, null)) // Malformed XML
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}

				if (typeof(IPrototype).IsAssignableFrom(elementType))
				{
					// Prototype ref
					this.elements.Add(new SerializedPrototypeReference()
					{
						name = xElementNode.Value
					});
				}
				else
				{
					// Determine type
					var serializableTypeCache = elementTypeCache;
					string typeName = elementTypeName;

					// Try to read class attrib
					var classAttrib = xElementNode.Attribute(PrototypeParser.PrototypeAttributeClass);
					if (!ReferenceEquals(classAttrib, null))
					{
						serializableTypeCache = PrototypesCaches.LookupSerializableTypeCache(classAttrib.Value, state.parameters.standardNamespace);
						typeName = classAttrib.Value;
					}

					// Validity checks
					// Field not serializable?
					if (ReferenceEquals(serializableTypeCache, null))
					{
						// TODO: Line number
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Collection element with unknown type " + typeName + " - unknown by the serializer cache! Are you missing " + nameof(PrototypesTypeSerializableAttribute) + " attribute? Skipping field!"));
						continue;
					}

					// Add element
					this.elements.Add(new SerializedData(serializableTypeCache, xElementNode, this.filename));
				}
			}

			foreach (var element in this.elements)
			{
				var sElement = element as SerializedData;
				if (!ReferenceEquals(sElement, null))
				{
					sElement.LoadFields(errors, state);
				}
			}
		}
		
		public object GetCollectionResolveReferenceFieldsAndSubData(List<IPrototype> prototypes, List<ParsingError> errors, PrototypeParserState state)
		{
			var collection = GetCollectionInstance(this.collectionType, this.elements.Count);
			for (int i = 0; i < this.elements.Count; i++)
			{
				// Finalize, create and apply
				var element = this.elements[i];
				var sElement = element as SerializedData;
				var protoRef = element as SerializedPrototypeReference;

				object value = null;
				if (!ReferenceEquals(sElement, null))
				{
					sElement.ResolveReferenceFieldsAndSubData(prototypes, errors, state);
					value = sElement.targetType.Create();
					sElement.ApplyTo(value, errors, state);
				}
				else if (!ReferenceEquals(protoRef, null))
				{
					value = protoRef.Resolve(prototypes);
				}
				else
					value = element;

				// Write to collection
				WriteElementToCollection(collection, value, i);
			}

			return collection;
		}
	}
}