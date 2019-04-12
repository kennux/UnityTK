using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Serialization.XML
{
	/// <summary>
	/// A cache datatype used to cache information on how to serialize types for the XML Serializer.
	/// </summary>
	public class SerializableTypeCache
	{
		private static Dictionary<Type, string> buildErrors = new Dictionary<Type, string>();

		/// <summary>
		/// Will retrieve a build error if there was one for the specified type.
		/// If there was no error, null will be returned. This is only used for diagnostic information output in <see cref="SerializerValidation"/>.
		/// 
		/// <see cref="TryBuild(Type)"/>
		/// </summary>
		public static string GetBuildError(Type type)
		{
			string str;
			if (!buildErrors.TryGetValue(type, out str))
				str = null;
			return str;
		}

		/// <summary>
		/// Tries buildding the cache for the specified type.
		/// In case of an issue while building, null will be returned.
		/// 
		/// If you want to know more as to why building failed, <see cref="GetBuildError(Type)"/>
		/// </summary>
		public static SerializableTypeCache TryBuild(Type type)
		{
			SerializableTypeCache stc = new SerializableTypeCache();

			try
			{
				stc.Build(type);
			}
			catch (Exception ex)
			{
				buildErrors.Set(type, ex.ToString());
				return null;
			}

			return stc;
		}

		public struct FieldCache
		{
			public FieldInfo fieldInfo;
			public SerializableTypeCache serializableTypeCache
			{
				get { return SerializerCache.GetSerializableTypeCacheFor(this.fieldInfo.FieldType); }
			}
			public bool isSerializableRoot
			{
				get { return typeof(ISerializableRoot).IsAssignableFrom(this.fieldInfo.FieldType); }
			}
			public bool isSerialized
			{
				get { return this.fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null; }
			}

			public FieldCache(FieldInfo fieldInfo)
			{
				this.fieldInfo = fieldInfo;
			}
		}

		public Type type
		{
			get;
			private set;
		}

		private SerializableTypeCache()
		{

		}

		public object Create()
		{
			return Activator.CreateInstance(this.type);
		}

		public void Build(Type type)
		{
			this.type = type;
		}

		public bool HasField(string fieldName)
		{
			return !ReferenceEquals(this.type.GetField(fieldName), null);
		}

		public FieldCache? GetFieldData(string fieldName)
		{
			var fi = this.type.GetField(fieldName);
			return ReferenceEquals(fi, null) ? (FieldCache?)null : new FieldCache(fi);
		}

		public void GetAllFields(List<FieldCache> outFields)
		{
			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
				outFields.Add(new FieldCache(field));
		}
	}
}