using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityTK.Prototypes;

namespace UnityTK.Examples.Prototypes
{
	public class TestPrototype : IPrototype
	{
		public string name;
		public float someRate;
		public int someInt;
		public TestPrototype someOtherPrototype = null;
		public Type type = null;

		string IPrototype.name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}

		public void PostLoad()
		{

		}
	}
}