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

		public void Serialize(object obj, XElement target, XMLSerializerParams parameters)
		{
			target.Value = Serialize((T)obj, parameters);
		}

		protected abstract T _Deserialize(string value, XMLSerializerParams parameters);
		protected abstract string Serialize(T value, XMLSerializerParams parameters);
	}
	
	public class XMLSerializer_Float : XMLSerializer_ValueTypeBase<float>
	{
		protected override string Serialize(float value, XMLSerializerParams parameters)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		protected override float _Deserialize(string value, XMLSerializerParams parameters)
		{
			return float.Parse(value, CultureInfo.InvariantCulture);
		}
	}
	
	public class XMLSerializer_Int : XMLSerializer_ValueTypeBase<int>
	{
		protected override string Serialize(int value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}

		protected override int _Deserialize(string value, XMLSerializerParams parameters)
		{
			return int.Parse(value);
		}
	}
	
	public class XMLSerializer_UInt : XMLSerializer_ValueTypeBase<uint>
	{
		protected override string Serialize(uint value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}

		protected override uint _Deserialize(string value, XMLSerializerParams parameters)
		{
			return uint.Parse(value);
		}
	}
	
	public class XMLSerializer_String : XMLSerializer_ValueTypeBase<string>
	{
		protected override string Serialize(string value, XMLSerializerParams parameters)
		{
			return value;
		}

		protected override string _Deserialize(string value, XMLSerializerParams parameters)
		{
			return value;
		}
	}
	
	public class XMLSerializer_Double : XMLSerializer_ValueTypeBase<double>
	{
		protected override string Serialize(double value, XMLSerializerParams parameters)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		protected override double _Deserialize(string value, XMLSerializerParams parameters)
		{
			return double.Parse(value, CultureInfo.InvariantCulture);
		}
	}
	
	public class XMLSerializer_Short : XMLSerializer_ValueTypeBase<short>
	{
		protected override string Serialize(short value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}
		protected override short _Deserialize(string value, XMLSerializerParams parameters)
		{
			return short.Parse(value);
		}
	}
	
	public class XMLSerializer_UShort : XMLSerializer_ValueTypeBase<ushort>
	{
		protected override string Serialize(ushort value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}
		protected override ushort _Deserialize(string value, XMLSerializerParams parameters)
		{
			return ushort.Parse(value);
		}
	}
	
	public class XMLSerializer_Byte : XMLSerializer_ValueTypeBase<byte>
	{
		protected override string Serialize(byte value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}
		protected override byte _Deserialize(string value, XMLSerializerParams parameters)
		{
			return byte.Parse(value);
		}
	}
	
	public class XMLSerializer_SByte : XMLSerializer_ValueTypeBase<sbyte>
	{
		protected override string Serialize(sbyte value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}
		protected override sbyte _Deserialize(string value, XMLSerializerParams parameters)
		{
			return sbyte.Parse(value);
		}
	}
	
	public class XMLSerializer_Bool : XMLSerializer_ValueTypeBase<bool>
	{
		protected override string Serialize(bool value, XMLSerializerParams parameters)
		{
			return value.ToString();
		}

		protected override bool _Deserialize(string value, XMLSerializerParams parameters)
		{
			return bool.Parse(value);
		}
	}
	
	public class XMLSerializer_Vector2 : XMLSerializer_ValueTypeBase<Vector2>
	{
		protected override string Serialize(Vector2 value, XMLSerializerParams parameters)
		{
			return string.Format("{0},{1}", value.x.ToString().Replace(',', '.'), value.y.ToString().Replace(',', '.'));
		}

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
		protected override string Serialize(Vector3 value, XMLSerializerParams parameters)
		{
			return string.Format("{0},{1},{2}", value.x.ToString().Replace(',', '.'), value.y.ToString().Replace(',', '.'), value.z.ToString().Replace(',', '.'));
		}

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
		protected override string Serialize(Vector4 value, XMLSerializerParams parameters)
		{
			return string.Format("{0},{1},{2},{3}", value.x.ToString().Replace(',', '.'), value.y.ToString().Replace(',', '.'), value.z.ToString().Replace(',', '.'), value.w.ToString().Replace(',', '.'));
		}

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
		protected override string Serialize(Color value, XMLSerializerParams parameters)
		{
			return string.Format("{0},{1},{2},{3}", value.r.ToString().Replace(',', '.'), value.g.ToString().Replace(',', '.'), value.b.ToString().Replace(',', '.'), value.a.ToString().Replace(',', '.'));
		}

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
		protected override string Serialize(Quaternion value, XMLSerializerParams parameters)
		{
			return string.Format("{0},{1},{2},{3}", value.x.ToString().Replace(',', '.'), value.y.ToString().Replace(',', '.'), value.z.ToString().Replace(',', '.'), value.w.ToString().Replace(',', '.'));
		}

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

		public void Serialize(object obj, XElement target, XMLSerializerParams parameters)
		{
			target.Value = obj.ToString();
		}
	}

	public class XMLSerializer_Type : XMLSerializer_ValueTypeBase<Type>
	{
		protected override string Serialize(Type value, XMLSerializerParams parameters)
		{
			return value.Name;
		}

		protected override Type _Deserialize(string value, XMLSerializerParams parameters)
		{
			// Create std namespace name string by prepending std namespace to value
			bool doStdNamespaceCheck = !string.IsNullOrWhiteSpace(parameters.standardNamespace);
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