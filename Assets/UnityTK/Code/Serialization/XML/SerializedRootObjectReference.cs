using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Serialization.XML
{
	class SerializedRootObjectReference
	{
		public string identifier;

		public ISerializableRoot Resolve(List<ISerializableRoot> objects)
		{
			foreach (var p in objects)
			{
				if (string.Equals(p.identifier, this.identifier))
					return p;
			}

			return null;
		}
	}
}