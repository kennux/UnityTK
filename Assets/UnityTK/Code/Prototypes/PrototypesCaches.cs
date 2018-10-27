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
		private static List<IPrototypeSerializer> serializers = new List<IPrototypeSerializer>();
		private static Dictionary<Type, SerializableTypeCache> typeCache = new Dictionary<Type, SerializableTypeCache>();
		private static bool wasInitialized = false;

		public static IPrototypeSerializer GetBestSerializerFor(Type type)
		{
			foreach (var instance in serializers)
			{
				if (instance.CanBeUsedFor(type))
					return instance;
			}
			return null;
		}

		public static SerializableTypeCache GetSerializableTypeCacheFor(Type type)
		{
			SerializableTypeCache cache;
			if (!typeCache.TryGetValue(type, out cache))
				return null;
			return cache;
		}

		public static SerializableTypeCache LookupSerializableTypeCache(string writtenName, string preferredNamespace)
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
					if (typeof(IPrototype).IsAssignableFrom(type) || type.GetCustomAttributes(true).Any((a) => a.GetType() == typeof(PrototypeDataSerializableAttribute)))
					{
						SerializableTypeCache cache = new SerializableTypeCache();
						cache.Build(type);
						typeCache.Add(type, cache);
					}
					else if (!type.IsAbstract && !type.IsInterface && typeof(IPrototypeSerializer).IsAssignableFrom(type))
					{
						serializers.Add(Activator.CreateInstance(type) as IPrototypeSerializer);
					}
				}
			}

			wasInitialized = true;
		}
	}
}
