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

		public object Deserialize(Type type, string value, PrototypeParserState state)
		{
			return _Deserialize(value, state);
		}

		protected abstract T _Deserialize(string value, PrototypeParserState state);
	}
	
	public class PrototypeSerializer_Float : PrototypeSerializer<float>
	{
		protected override float _Deserialize(string value, PrototypeParserState state)
		{
			return float.Parse(value);
		}
	}
	
	public class PrototypeSerializer_Int : PrototypeSerializer<int>
	{
		protected override int _Deserialize(string value, PrototypeParserState state)
		{
			return int.Parse(value);
		}
	}
	
	public class PrototypeSerializer_String : PrototypeSerializer<string>
	{
		protected override string _Deserialize(string value, PrototypeParserState state)
		{
			return value;
		}
	}
	
	public class PrototypeSerializer_Double : PrototypeSerializer<double>
	{
		protected override double _Deserialize(string value, PrototypeParserState state)
		{
			return double.Parse(value);
		}
	}
	
	public class PrototypeSerializer_Short : PrototypeSerializer<short>
	{
		protected override short _Deserialize(string value, PrototypeParserState state)
		{
			return short.Parse(value);
		}
	}
	
	public class PrototypeSerializer_Byte : PrototypeSerializer<byte>
	{
		protected override byte _Deserialize(string value, PrototypeParserState state)
		{
			return byte.Parse(value);
		}
	}
	
	public class PrototypeSerializer_Bool : PrototypeSerializer<bool>
	{
		protected override bool _Deserialize(string value, PrototypeParserState state)
		{
			return bool.Parse(value);
		}
	}

	public class PrototypeSerializer_Enum : IPrototypeSerializer
	{
		public bool CanBeUsedFor(Type type)
		{
			return type.IsEnum;
		}

		public object Deserialize(Type type, string value, PrototypeParserState state)
		{
			return Enum.Parse(type, value);
		}
	}

	public class PrototypeSerializer_Type : PrototypeSerializer<Type>
	{
		protected override Type _Deserialize(string value, PrototypeParserState state)
		{
			// TODO: Improve!
			return Type.GetType(value);
		}
	}
}