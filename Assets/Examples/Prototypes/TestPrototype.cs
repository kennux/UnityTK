using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityTK.Prototypes;

namespace UnityTK.Examples.Prototypes
{
	[System.Serializable]
	public class TestPrototype : IPrototype
	{
		[PrototypesTypeSerializableAttribute]
		[System.Serializable]
		public struct TestStruct
		{
			public int test;
		}
		
		[PrototypesTypeSerializableAttribute]
		public class TestBase
		{
			public string baseStr;
		}
		
		[PrototypesTypeSerializableAttribute]
		public class SpecializedClass : TestBase
		{
			public int lul;
		}

		public string name;
		public float someRate;
		public int someInt;
		public TestPrototype someOtherPrototype = null;
		public Type type = null;
		public TestStruct _struct;
		public TestBase testBase;
		
		public TestBase[] array;
		public List<TestBase> list;
		public HashSet<TestBase> hashSet;

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
	}
}