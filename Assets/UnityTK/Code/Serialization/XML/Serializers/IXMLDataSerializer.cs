using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace UnityTK.Serialization.XML
{
	/// <summary>
	/// These serializers are used by <see cref="XMLSerializer"/> in order to serialize data types.
	/// </summary>
	public interface IXMLDataSerializer
	{
		/// <summary>
		/// Whether or not this serializer can be used for the specified type.
		/// </summary>
		bool CanBeUsedFor(Type type);

		/// <summary>
		/// Deserializes an object of the specified type using the xml element passed in.
		/// </summary>
		/// <param name="type">The type to deserialize value to.</param>
		/// <param name="value">The serialized value read from XML</param>
		/// <param name="parameters">Current serializer parameters</param>
		/// <returns>The deserialized object</returns>
		object Deserialize(Type type, XElement value, XMLSerializerParams parameters);

		/// <summary>
		/// Serializes an object into the specified XElement target.
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="target">To element to serialize to</param>
		/// <param name="parameters">Current serializer parameters</param>
		void Serialize(object obj, XElement target, XMLSerializerParams parameters);
	}
}