using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Prototypes
{
	public interface IPrototypeSerializer
	{
		object Deserialize(string value);
	}
}