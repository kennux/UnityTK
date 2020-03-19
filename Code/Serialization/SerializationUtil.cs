using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace UnityTK.Serialization
{
	public static class SerializationUtil
	{
		public static string GetRandomId()
		{
			return System.Guid.NewGuid().ToString();
		}

		public static void AssignRandomIds(List<ISerializableRoot> roots)
		{
			foreach (var root in roots)
				root.identifier = GetRandomId();
		}
	}
}
