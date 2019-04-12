using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Serialization.XML
{
	internal static class SerializerValidation
	{
		public static bool DataFieldSerializerValid(XMLSerializerParams parameters, XElement xElement, SerializableTypeCache typeCache, string typeName, string fieldName, string filename, List<SerializerError> errors)
		{
			if (ReferenceEquals(typeCache, null))
			{
				// TODO: Explanation - why unserializable!?
				string msg = string.Format("Field '{0}' with unserializeable type {1}!", fieldName, typeName);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}
		
		public static bool TypeCheck(XMLSerializerParams parameters, IXmlLineInfo debug, string fieldName, object value, Type expectedType, string filename, List<SerializerError> errors)
		{
			if (!ReferenceEquals(value, null) && !expectedType.IsAssignableFrom(value.GetType()))
			{
				string msg = string.Format("Fatal error deserializing field {0} - tried applying field data but types mismatched! Stored type: {1} - Expected type: {2}!", fieldName, value.GetType(), expectedType);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool ContainerElementName(XMLSerializerParams parameters, XElement xElement, string filename, List<SerializerError> errors)
		{
			if (!string.Equals(xElement.Name.LocalName, parameters.rootElementName))
			{
				string msg = string.Format("Element name '{0}' is incorrect / not supported, must be '{1}'!", xElement.Name, parameters.rootElementName);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool ElementHasId(XMLSerializerParams parameters, XElement xElement, string filename, List<SerializerError> errors)
		{
			var attribName = xElement.Attribute(XMLSerializer.AttributeIdentifier);
			if (ReferenceEquals(attribName, null))
			{
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Root object without identifier!"));
				return false;
			}
			return true;
		}

		public static bool RootTypeFound(XMLSerializerParams parameters, string typeName, XElement xElement, SerializableTypeCache type, string filename, List<SerializerError> errors)
		{
			if (ReferenceEquals(type, null))
			{
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Root object type " + typeName + " unknown!"));
				return false;
			}
			return true;
		}

		public static bool TypeFound(XMLSerializerParams parameters, XElement xElement, XAttribute typeAttribute, SerializableTypeCache type, string filename, List<SerializerError> errors)
		{
			if (ReferenceEquals(type, null))
			{
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element type " + typeAttribute.Value + " unknown!"));
				return false;
			}
			return true;
		}

		public static bool NodeIsElement(XMLSerializerParams parameters, XNode xNode, string filename, List<SerializerError> errors)
		{
			if (!(xNode is XElement)) // Malformed XML
			{
				string msg = string.Format("Unable to cast node to element for {0}!", xNode);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, (xNode as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool SerializerWasFound(XMLSerializerParams parameters, IXmlLineInfo debug, IXMLDataSerializer serializer, string field, Type declaringType, Type fieldType, string filename, List<SerializerError> errors)
		{
			if (ReferenceEquals(serializer, null))
			{
				// TODO: Line number
				string msg = string.Format("Serializer for field {0} on type {1} (Field type: {2}) could not be found!", field, declaringType, fieldType);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool FieldKnown(XMLSerializerParams parameters, IXmlLineInfo debug, SerializableTypeCache type, string field, string filename, List<SerializerError> errors)
		{
			if (!type.HasField(field))
			{
				// TODO: Line number
				string msg = string.Format("Unknown field {0}!", field);
				errors.Add(new SerializerError(SerializerErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}
	}
}
