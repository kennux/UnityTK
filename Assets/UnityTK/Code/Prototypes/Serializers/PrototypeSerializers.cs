using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Prototypes
{
	public abstract class PrototypeSerializer<T> : IPrototypeSerializer
	{
		public bool CanBeUsedFor(Type type)
		{
			return ReferenceEquals(type, typeof(T));
		}

		public object Deserialize(string value, PrototypeParserState state)
		{
			return _Deserialize(value, state);
		}

		protected abstract T _Deserialize(string value, PrototypeParserState state);
	}
	
	public class PrototypeFloatSerializer : PrototypeSerializer<float>
	{
		protected override float _Deserialize(string value, PrototypeParserState state)
		{
			return float.Parse(value);
		}
	}
	
	public class PrototypeIntSerializer : PrototypeSerializer<int>
	{
		protected override int _Deserialize(string value, PrototypeParserState state)
		{
			return int.Parse(value);
		}
	}
	
	public class PrototypeStringSerializer : PrototypeSerializer<string>
	{
		protected override string _Deserialize(string value, PrototypeParserState state)
		{
			return value;
		}
	}
	
	public class PrototypeTypeSerializer : PrototypeSerializer<Type>
	{
		protected override Type _Deserialize(string value, PrototypeParserState state)
		{
			return Type.GetType(value);
		}
	}
}