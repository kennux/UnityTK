using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Static utility class providing helper methods for several tasks.
    /// </summary>
    public static class Essentials
    {
        /// <summary>
        /// Helper method that does "unity" null checks.
        /// Since fields which are referencing components which are null arent actually null in unity, this is a specialized null equality check to deal with this nonsense.
        /// </summary>
        public static bool UnityIsNull(object obj)
        {
            return object.ReferenceEquals(obj, null) || obj.Equals(null);
        }
        
        /// <summary>
        /// Tries getting the value for the specified key from the dictionary.
        /// If the value is not found, its created via new TValue().
        /// </summary>
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;
            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        /// <summary>
        /// Sets the specified key in the dictionary to the specified value.
        /// 
        /// If the key is not existing yet, a new entry is created in the dictionary.
        /// If it already exists, its being overwritten.
        /// </summary>
        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        private static BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static T DeserializeBinaryFormatted<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return (T)binaryFormatter.Deserialize(ms);
            }
        }

        public static byte[] SerializeBinaryFormatted<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
