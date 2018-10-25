using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Prototypes
{
	public abstract class PrototypeSerializer<T> : IPrototypeSerializer
	{
		public object Deserialize(string value)
		{
			return _Deserialize(value);
		}

		protected abstract T _Deserialize(string value);
	}

	[PrototypesTypeSerializerAttribute(typeof(float))]
	public class PrototypeFloatSerializer : PrototypeSerializer<float>
	{
		protected override float _Deserialize(string value)
		{
			return float.Parse(value);
		}
	}

	[PrototypesTypeSerializerAttribute(typeof(int))]
	public class PrototypeIntSerializer : PrototypeSerializer<int>
	{
		protected override int _Deserialize(string value)
		{
			return int.Parse(value);
		}
	}

	[PrototypesTypeSerializerAttribute(typeof(string))]
	public class PrototypeStringSerializer : PrototypeSerializer<string>
	{
		protected override string _Deserialize(string value)
		{
			return value;
		}
	}

	[PrototypesTypeSerializerAttribute(typeof(Type))]
	public class PrototypeTypeSerializer : PrototypeSerializer<Type>
	{
		protected override Type _Deserialize(string value)
		{
			return Type.GetType(value);
		}
	}
}