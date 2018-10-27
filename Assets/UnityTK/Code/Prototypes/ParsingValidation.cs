using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Prototypes
{
	internal static class ParsingValidation
	{
		// TODO: string.Format()

		public static bool DataFieldSerializerFound(XElement xElement, SerializableTypeCache typeCache, string typeName, string fieldName, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(typeCache, null))
			{
				// TODO: Line number
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Field '" + fieldName + "' with unknown type " + typeName + " - unknown by the serializer cache! Are you missing " + nameof(PrototypeDataSerializableAttribute) + " attribute? Skipping field!"));
				return false;
			}
			return true;
		}

		// TODO: XElement for line number!
		public static bool TypeCheck(string fieldName, object value, Type expectedType, string filename, List<ParsingError> errors)
		{
			if (!ReferenceEquals(value, null) && !expectedType.IsAssignableFrom(value.GetType()))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Fatal error deserializing field " + fieldName + " - tried applying field data but types mismatched! Stored type: " + value.GetType() + " - Expected type: " + expectedType + "! Skipping field!"));
				return false;
			}
			return true;
		}

		public static bool ContainerElementName(XElement xElement, string filename, List<ParsingError> errors)
		{
			if (!string.Equals(xElement.Name.LocalName, PrototypeParser.PrototypeContainerXMLName))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element name '" + xElement.Name + "' is incorrect / not supported, must be '"+PrototypeParser.PrototypeContainerXMLName+"'!"));
				return false;
			}
			return true;
		}

		public static bool PrototypeElementName(XElement xElement, string filename, List<ParsingError> errors)
		{
			if (!string.Equals(xElement.Name.LocalName, PrototypeParser.PrototypeElementXMLName)) // Unsupported
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element name '" + xElement.Name + "' is incorrect / not supported, must be '" + PrototypeParser.PrototypeElementXMLName + "'!"));
				return false;
			}
			return true;
		}

		public static bool ContainerTypeAttribute(XElement xElement, string filename, List<ParsingError> errors)
		{
			var typeAttribute = xElement.Attribute(PrototypeParser.PrototypeContainerAttributeType);
			if (ReferenceEquals(typeAttribute, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element missing '"+PrototypeParser.PrototypeContainerAttributeType+"'! Need '"+PrototypeParser.PrototypeContainerAttributeType+"' attribute specifying the type of the prototypes to be loaded!"));
				return false;
			}
			return true;
		}

		public static bool ElementHasId(XElement xElement, string filename, List<ParsingError> errors)
		{
			var attribName = xElement.Attribute(PrototypeParser.PrototypeAttributeIdentifier);
			if (ReferenceEquals(attribName, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Prototype without identifier!"));
				return false;
			}
			return true;
		}

		public static bool TypeFound(XElement xElement, XAttribute typeAttribute, SerializableTypeCache type, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(type, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element type " + typeAttribute.Value + " unknown!"));
				return false;
			}
			return true;
		}

		public static bool NodeIsElement(XNode xNode, string filename, List<ParsingError> errors)
		{
			if (!(xNode is XElement)) // Malformed XML
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xNode as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + xNode + "!"));
				return false;
			}
			return true;
		}

		public static bool SerializerWasFound(IPrototypeDataSerializer serializer, string field, Type declaringType, Type fieldType, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(serializer, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer for field " + field + " on type " + declaringType + " (Field type: " + fieldType + ") could not be found! Skipping field!"));
				return false;
			}
			return true;
		}

		public static bool FieldKnown(SerializableTypeCache type, string field, string filename, List<ParsingError> errors)
		{
			if (!type.HasField(field))
			{
				// TODO: Line number
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Unknown field " + field + "!"));
				return false;
			}
			return true;
		}
	}
}
