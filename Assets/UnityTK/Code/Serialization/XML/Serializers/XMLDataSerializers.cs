using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;

namespace UnityTK.Serialization.XML
{
	public abstract class XMLSerializer_ValueTypeBase<T> : IXMLDataSerializer
	{
		public bool CanBeUsedFor(Type type)
		{
			return ReferenceEquals(type, typeof(T));
		}

		public object Deserialize(Type type, XElement value, XMLSerializerParams parameters)
		{
			return _Deserialize(value.Value, parameters);
		}

		protected abstract T _Deserialize(string value, XMLSerializerParams parameters);
	}
	
	public class XMLSerializer_Float : XMLSerializer_ValueTypeBase<float>
	{
		protected override float _Deserialize(string value, XMLSerializerParams parameters)
		{
			return float.Parse(value, CultureInfo.InvariantCulture);
		}
	}
	
	public class XMLSerializer_Int : XMLSerializer_ValueTypeBase<int>
	{
		protected override int _Deserialize(string value, XMLSerializerParams parameters)
		{
			return int.Parse(value);
		}
	}
	
	public class XMLSerializer_String : XMLSerializer_ValueTypeBase<string>
	{
		protected override string _Deserialize(string value, XMLSerializerParams parameters)
		{
			return value;
		}
	}
	
	public class XMLSerializer_Double : XMLSerializer_ValueTypeBase<double>
	{
		protected override double _Deserialize(string value, XMLSerializerParams parameters)
		{
			return double.Parse(value, CultureInfo.InvariantCulture);
		}
	}
	
	public class XMLSerializer_Short : XMLSerializer_ValueTypeBase<short>
	{
		protected override short _Deserialize(string value, XMLSerializerParams parameters)
		{
			return short.Parse(value);
		}
	}
	
	public class XMLSerializer_Byte : XMLSerializer_ValueTypeBase<byte>
	{
		protected override byte _Deserialize(string value, XMLSerializerParams parameters)
		{
			return byte.Parse(value);
		}
	}
	
	public class XMLSerializer_Bool : XMLSerializer_ValueTypeBase<bool>
	{
		protected override bool _Deserialize(string value, XMLSerializerParams parameters)
		{
			return bool.Parse(value);
		}
	}
	
	public class XMLSerializer_Vector2 : XMLSerializer_ValueTypeBase<Vector2>
	{
		protected override Vector2 _Deserialize(string value, XMLSerializerParams parameters)
		{
			string[] parts = value.Split(',');
			if (parts.Length < 2)
				throw new FormatException("Malformed vector2 data " + value + " - Format: (x,y - . as decimal delimiter)");

			return new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));
		}
	}
	
	public class XMLSerializer_Vector3 : XMLSerializer_ValueTypeBase<Vector3>
	{
		protected override Vector3 _Deserialize(string value, XMLSerializerParams parameters)
		{
			string[] parts = value.Split(',');
			if (parts.Length < 3)
				throw new FormatException("Malformed vector3 data " + value + " - Format: (x,y,z - . as decimal delimiter)");

			return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture),
				float.Parse(parts[2], CultureInfo.InvariantCulture));
		}
	}
	
	public class XMLSerializer_Vector4 : XMLSerializer_ValueTypeBase<Vector4>
	{
		protected override Vector4 _Deserialize(string value, XMLSerializerParams parameters)
		{
			string[] parts = value.Split(',');
			if (parts.Length < 4)
				throw new FormatException("Malformed vector4 data " + value + " - Format: (x,y,z,w - . as decimal delimiter)");

			return new Vector4(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture),
				float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture));
		}
	}
	
	public class XMLSerializer_Color : XMLSerializer_ValueTypeBase<Color>
	{
		protected override Color _Deserialize(string value, XMLSerializerParams parameters)
		{
			string[] parts = value.Split(',');
			if (parts.Length < 4)
				throw new FormatException("Malformed color data " + value + " - Format: (x,y,z,w - . as decimal delimiter)");

			return new Color(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture),
				float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture));
		}
	}
	
	public class XMLSerializer_Quaternion : XMLSerializer_ValueTypeBase<Quaternion>
	{
		protected override Quaternion _Deserialize(string value, XMLSerializerParams parameters)
		{
			string[] parts = value.Split(',');
			if (parts.Length < 4)
				throw new FormatException("Malformed quaternion data " + value + " - Format: (x,y,z,w - . as decimal delimiter)");

			return new Quaternion(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture),
				float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture));
		}
	}

	public class XMLSerializer_Enum : IXMLDataSerializer
	{
		public bool CanBeUsedFor(Type type)
		{
			return type.IsEnum;
		}

		public object Deserialize(Type type, XElement value, XMLSerializerParams parameters)
		{
			return Enum.Parse(type, value.Value as string);
		}
	}

	public class XMLSerializer_Type : XMLSerializer_ValueTypeBase<Type>
	{
		protected override Type _Deserialize(string value, XMLSerializerParams parameters)
		{
			// Create std namespace name string by prepending std namespace to value
			bool doStdNamespaceCheck = !value.Contains('.');
			string stdNamespacePrepended = doStdNamespaceCheck ? parameters.standardNamespace + "." + value : null;

			// Look for type
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				var t = asm.GetType(value, false, false);
				if (doStdNamespaceCheck && ReferenceEquals(t, null))
					t = asm.GetType(stdNamespacePrepended, false, false);

				if (!ReferenceEquals(t, null))
					return t;
			}

			return null;
		}
	}
}