using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Linq;

namespace UnityTK.Serialization.XML
{
	public static class SerializerCache
	{
		private static List<IXMLDataSerializer> serializers;
		private static Dictionary<Type, SerializableTypeCache> typeCache = new Dictionary<Type, SerializableTypeCache>();
		private static Type[] allTypes = null;

		/// <summary>
		/// Returns the best known data serializer for the specified type.
		/// Currently this will always return the first serializer found, TODO: Implement serializer rating and selecting the most appropriate one.
		/// </summary>
		/// <param name="type">The type to get a serializer for.</param>
		/// <returns>Null if not found, the serializer otherwise.</returns>
		public static IXMLDataSerializer GetBestSerializerFor(Type type)
		{
			if (ReferenceEquals(serializers, null))
			{
				serializers = new List<IXMLDataSerializer>();
				LazyAllTypesInit();
				
				int len = allTypes.Length;
				for (int i = 0; i < len; i++)
				{
					Type t = allTypes[i];
					if (t.IsClass && !t.IsAbstract && typeof(IXMLDataSerializer).IsAssignableFrom(t))
						serializers.Add(Activator.CreateInstance(t) as IXMLDataSerializer);
				}
			}

			foreach (var instance in serializers)
			{
				if (instance.CanBeUsedFor(type))
					return instance;
			}
			return null;
		}

		/// <summary>
		/// Returns the serializable type cache if known for the specified type.
		/// Will be cached just in time.
		/// </summary>
		public static SerializableTypeCache GetSerializableTypeCacheFor(Type type)
		{
			SerializableTypeCache cache;
			if (!typeCache.TryGetValue(type, out cache))
			{
				cache = SerializableTypeCache.TryBuild(type);
				typeCache.Add(type, cache);
			}
			return cache;
		}

		private static void LazyAllTypesInit()
		{
			if (ReferenceEquals(allTypes, null))
			{
				List<Type> types = new List<Type>();
				// Init all types cache
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					try
					{
						foreach (var type in asm.GetTypes())
						{
							types.Add(type);
						}
					}
					catch (Exception ex) { Debug.LogError("Exception while initializing types of " + asm + " for serialization!"); Debug.LogException(ex); }
				}
				allTypes = types.ToArray();
			}
		}

		struct TypeCacheKey : IEquatable<TypeCacheKey>
		{
			public string writtenName;
			public string preferredNamespace;

			public bool Equals(TypeCacheKey other)
			{
				return string.Equals(writtenName, other.writtenName) && string.Equals(preferredNamespace, other.preferredNamespace);
			}

			public override int GetHashCode()
			{
				return Essentials.CombineHashCodes(writtenName.GetHashCode(), preferredNamespace == null ? 0 : preferredNamespace.GetHashCode());
			}

			public TypeCacheKey(string writtenName, string preferredNamespace)
			{
				this.writtenName = writtenName;
				this.preferredNamespace = preferredNamespace;
			}
		}

		private static Dictionary<TypeCacheKey, Type> _serializableTypeCache = new Dictionary<TypeCacheKey, Type>();
		public static SerializableTypeCache GetSerializableTypeCacheFor(string writtenName, string preferredNamespace)
		{
			TypeCacheKey cacheKey = new TypeCacheKey(writtenName, preferredNamespace);
			Type foundType = null;
			if (!_serializableTypeCache.TryGetValue(cacheKey, out foundType))
			{
				bool dontDoNamespaceCheck = string.IsNullOrEmpty(preferredNamespace);
				LazyAllTypesInit();

				int len = allTypes.Length;
				for (int i = 0; i < len; i++)
				{
					Type t = allTypes[i];
					if (t.Name.Equals(writtenName) && (dontDoNamespaceCheck || t.Namespace.Equals(preferredNamespace)))
					{
						foundType = t;
						break;
					}
				}

				if (foundType == null && !dontDoNamespaceCheck)
				{
					var t = GetSerializableTypeCacheFor(writtenName, null);
					if (t != null)
						return t;
				}

				_serializableTypeCache.Add(cacheKey, foundType);
			}

			if (!ReferenceEquals(foundType, null))
				return GetSerializableTypeCacheFor(foundType);

			return null;
		}
	}
}
