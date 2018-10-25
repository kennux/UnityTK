using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Prototypes
{
	public interface IPrototype
	{
		string name { get; set; }
		void PostLoad();
	}
}