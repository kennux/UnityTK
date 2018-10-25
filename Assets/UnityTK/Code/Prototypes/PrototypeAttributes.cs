using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Prototypes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PrototypesTypeSerializerAttribute : Attribute
	{
		public Type valueType;

		public PrototypesTypeSerializerAttribute(Type valueType)
		{
			this.valueType = valueType;
		}
	}
}