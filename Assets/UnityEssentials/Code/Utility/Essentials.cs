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
