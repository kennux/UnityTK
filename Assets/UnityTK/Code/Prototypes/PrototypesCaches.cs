using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Linq;

namespace UnityTK.Prototypes
{
	internal static class PrototypesCaches
	{
		private static Dictionary<Type, IPrototypeSerializer> serializers = new Dictionary<Type, IPrototypeSerializer>();
		private static Dictionary<Type, SerializableTypeCache> typeCache = new Dictionary<Type, SerializableTypeCache>();
		private static bool wasInitialized = false;

		public static IPrototypeSerializer GetSerializerFor(Type type)
		{
			IPrototypeSerializer serializer;
			if (!serializers.TryGetValue(type, out serializer))
				return null;
			return serializer;
		}

		public static SerializableTypeCache GetTypeCacheFor(Type type)
		{
			SerializableTypeCache cache;
			if (!typeCache.TryGetValue(type, out cache))
				return null;
			return cache;
		}

		public static SerializableTypeCache LookupTypeCache(string writtenName, string preferredNamespace)
		{
			List<SerializableTypeCache> tmp = ListPool<SerializableTypeCache>.Get();

			try
			{
				foreach (var cache in typeCache)
				{
					if (string.Equals(cache.Key.Name, writtenName))
						tmp.Add(cache.Value);
				}

				if (tmp.Count > 0)
				{
					foreach (var cache in tmp)
						if (string.Equals(cache.type.Namespace, preferredNamespace))
							return cache;
				}

				return tmp.Count == 0 ? null : tmp[0];
			}
			finally
			{
				ListPool<SerializableTypeCache>.Return(tmp);
			}
		}

		public static void LazyInit()
		{
			if (wasInitialized)
				return;

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in asm.GetTypes())
				{
					if (typeof(IPrototype).IsAssignableFrom(type))
					{
						SerializableTypeCache cache = new SerializableTypeCache();
						cache.Build(type);
						typeCache.Add(type, cache);
					}
					else if (!type.IsAbstract && !type.IsInterface && typeof(IPrototypeSerializer).IsAssignableFrom(type))
					{
						var attrib = type.GetCustomAttributes(false).Where((a) => (a is PrototypesTypeSerializerAttribute)).FirstOrDefault();
						if (ReferenceEquals(attrib, null))
							Debug.LogError("Value type serializer for UnityTK prototypes without " + nameof(PrototypesTypeSerializerAttribute) + " detected! Class: " + type.Name);
						else
						{
							var attribCasted = attrib as PrototypesTypeSerializerAttribute;
							serializers.Add(attribCasted.valueType, Activator.CreateInstance(type) as IPrototypeSerializer);
						}
					}
				}
			}

			wasInitialized = true;
		}
	}
}
