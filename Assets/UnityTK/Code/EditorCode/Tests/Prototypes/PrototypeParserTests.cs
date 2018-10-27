using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityTK.Prototypes;

namespace UnityTK.Test.Prototypes
{
	public class TestPrototypeSpec : TestPrototype
	{
		public int testField;
	}
	
	public class TestPrototype : IPrototype
	{
		[PrototypesTypeSerializableAttribute]
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

    public class DataBindingNodesTest
    {
        [Test]
        public void ParserTestValueTypes()
        {
			string xml = "<PrototypeContainer Type=\"TestPrototype\">\n" +
				"	<Prototype Id=\"Test\">\n" +
				"		<someRate>2.5</someRate>\n" +
				"		<someInt>5</someInt>\n" +
				"	</Prototype>\n" +
				"</PrototypeContainer>";

			List<ParsingError> errors = new List<ParsingError>();
			var prototypes = PrototypeParser.Parse(xml, new PrototypeParseParameters()
			{
				standardNamespace = "UnityTK.Prototypes.Editor.Test"
			}, ref errors);

			Assert.AreEqual(1, prototypes.Count);
			foreach (var error in errors)
				throw new Exception(error.GetFullMessage());
			
			Assert.AreEqual(2.5f, (prototypes[0] as TestPrototype).someRate);
			Assert.AreEqual(5f, (prototypes[0] as TestPrototype).someInt);
        }

        [Test]
        public void ParserTestOverrideValueTypes()
        {
			string xml = "<PrototypeContainer Type=\"TestPrototype\">\n" +
				"	<Prototype Id=\"Test\" Abstract=\"True\">\n" +
				"		<someRate>2.5</someRate>\n" +
				"		<someInt>5</someInt>\n" +
				"	</Prototype>\n" +
				"	<Prototype Id=\"Test2\" Inherits=\"Test\">\n" +
				"		<someRate>4</someRate>\n" +
				"	</Prototype>\n" +
				"</PrototypeContainer>";

			List<ParsingError> errors = new List<ParsingError>();
			var prototypes = PrototypeParser.Parse(xml, new PrototypeParseParameters()
			{
				standardNamespace = "UnityTK.Prototypes.Editor.Test"
			}, ref errors);

			Assert.AreEqual(1, prototypes.Count);
			foreach (var error in errors)
				throw new Exception(error.GetFullMessage());
			
			Assert.AreEqual(4f, (prototypes[0] as TestPrototype).someRate);
			Assert.AreEqual(5f, (prototypes[0] as TestPrototype).someInt);
        }
    }
}