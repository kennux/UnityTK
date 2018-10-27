using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityTK.Prototypes;

namespace UnityTK.Examples.Prototypes
{
	public class TestPrototypeSpec : TestPrototype
	{
		public int testField;
	}

	[System.Serializable]
	public class TestPrototype : IPrototype
	{
		[PrototypeDataSerializable]
		[System.Serializable]
		public struct TestStruct
		{
			public int test;
		}
		
		[PrototypeDataSerializable]
		public class TestBase
		{
			public string baseStr;
		}
		
		[PrototypeDataSerializable]
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
		public TestPrototype[] arrayRefs;

		string IPrototype.identifier
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