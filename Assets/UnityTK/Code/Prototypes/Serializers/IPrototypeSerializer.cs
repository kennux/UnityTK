using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// Interface to be used for prototype serializers.
	/// 
	/// These serializers are used by <see cref="Prototypes"/> in order to serialize field data.
	/// </summary>
	public interface IPrototypeSerializer
	{
		/// <summary>
		/// Whether or not this serializer can be used for the specified type.
		/// </summary>
		bool CanBeUsedFor(Type type);

		object Deserialize(Type type, string value, PrototypeParserState state);
	}
}