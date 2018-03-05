using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEssentials
{
    /// <summary>
    /// Static reflection cache class.
    /// Provides cached access to the reflection api and some helper methods to work with reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        private static Dictionary<Type, List<Type>> assignableToCache = new Dictionary<Type, List<Type>>();

        public static string TypeToString(Type t)
        {
            return t.AssemblyQualifiedName;
        }

        public static Type TypeFromString(string s)
        {
            return Type.GetType(s);
        }

        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        public static List<Type> GetAllTypesAssignableTo(Type assignableTo, List<Type> preAlloc = null)
        {
            if (preAlloc == null)
                preAlloc = new List<Type>();

            List<Type> lst;
            if (assignableToCache.TryGetValue(assignableTo, out lst))
            {
                preAlloc.AddRange(lst);
                return preAlloc;
            }

            lst = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                {
                    if (assignableTo.IsAssignableFrom(type))
                    {
                        lst.Add(type);
                    }
                }

            assignableToCache.Add(assignableTo, lst);
            preAlloc.AddRange(lst);
            return preAlloc;
        }
    }
}