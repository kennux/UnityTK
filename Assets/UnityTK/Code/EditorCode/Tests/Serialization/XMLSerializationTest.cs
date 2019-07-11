using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityTK.Serialization.Prototypes;
using UnityTK.Serialization;
using System.Linq;

namespace UnityTK.Test.Serialization
{
#pragma warning disable CS0659 // Warning about overriding equals but not GetHashCode, this doesn't really matter for this test.
    public class XMLSerializationTest
	{
		class TestRoot : ISerializableRoot
		{
			public string identifier { get; set; }
		}

		class CollectionTest : TestRoot
		{
			public ValueTypeTestNoRoot[] test;
			public List<ValueTypeTest> test2;
			public List<int> test3;

			public override bool Equals(object obj)
			{
				var casted = obj as CollectionTest;
				if (casted == null)
					return false;

				if (casted.identifier != identifier)
					return false;

				if (test.Length != casted.test.Length)
					return false;

				if (test2.Count != casted.test2.Count)
					return false;

				if (test3.Count != casted.test3.Count)
					return false;

				for (int i = 0; i < test.Length; i++)
					if (!test[i].Equals(casted.test[i]))
						return false;

				for (int i = 0; i < test2.Count; i++)
					if (!test2[i].Equals(casted.test2[i]))
						return false;

				for (int i = 0; i < test3.Count; i++)
					if (test3[i] != casted.test3[i])
						return false;
				return true;
			}
		}

		class ValueTypeTestNoRoot
		{
			public int integer;

			public override bool Equals(object obj)
			{
				var casted = obj as ValueTypeTestNoRoot;
				if (casted == null)
					return false;
				
				return casted.integer == this.integer;
			}
		}

		class ValueTypeTest : TestRoot
		{
			public int integer;

			public override bool Equals(object obj)
			{
				var casted = obj as ValueTypeTest;
				if (casted == null)
					return false;
				
				return casted.identifier == this.identifier && casted.integer == this.integer;
			}
		}

		class NonSerializedTest : TestRoot
		{
			public int integer;
			[NonSerialized]
			public int integer2 = -1;
			[UnityTKNonSerialized]
			public int integer3 = -1;

			public override bool Equals(object obj)
			{
				var casted = obj as NonSerializedTest;
				if (casted == null)
					return false;
				
				return casted.identifier == this.identifier && casted.integer == this.integer;
			}
		}

		class AlwaysSerializedTest : TestRoot
		{
			[AlwaysSerialized]
			public string test = "test";

			public override bool Equals(object obj)
			{
				var casted = obj as AlwaysSerializedTest;
				if (casted == null)
					return false;
				
				return casted.identifier == this.identifier && casted.test == this.test;
			}
		}

		class ReferenceTest : TestRoot
		{
			public string someStr;
			public ReferenceTest someRef;

			public override bool Equals(object obj)
			{
				var casted = obj as ReferenceTest;
				if (casted == null)
					return false;
				
				return casted.identifier == this.identifier && casted.someStr == this.someStr &&
					((someRef != null && someRef.Equals(casted.someRef)) || ReferenceEquals(someRef, casted.someRef));
			}
		}

		class PolymorphismTestBase
		{

		}

		class PolymorphismTest1 : PolymorphismTestBase
		{
			public short test;

			public override bool Equals(object obj)
			{
				var casted = obj as PolymorphismTest1;
				if (casted == null)
					return false;
				
				return test == casted.test;
			}
		}

		class PolymorphismTest2 : PolymorphismTestBase
		{
			public uint test;

			public override bool Equals(object obj)
			{
				var casted = obj as PolymorphismTest2;
				if (casted == null)
					return false;

				return test == casted.test;
			}
		}

		class PolymorphismTestRoot : TestRoot
		{
			public PolymorphismTestBase test;

			public override bool Equals(object obj)
			{
				var casted = obj as PolymorphismTestRoot;
				if (casted == null)
					return false;
				
				return casted.identifier == this.identifier && test.Equals(casted.test);
			}
		}

		ISerializer CreateSerializer()
		{
			return new XMLSerializer(new XMLSerializerParams()
			{
				rootElementName = "TestData",
				standardNamespace = "UnityTK.Test.Serialization"
			});
		}

		private static List<SerializerError> errors;
		
		private void FlushErrors()
		{
			if (errors == null)
				return;

			foreach (var error in errors)
				error.DoUnityDebugLog();
		}

        [Test]
		public void TestValueTypes()
		{
			var serializer = CreateSerializer();
			ValueTypeTest test = new ValueTypeTest();
			test.integer = 1337;

			List<ISerializableRoot> roots = new List<ISerializableRoot>();
			roots.Add(test);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, null, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 1);
			Assert.AreEqual(test, roots[0]);
		}

        [Test]
		public void TestCollections()
		{
			var serializer = CreateSerializer();
			ValueTypeTest _test = new ValueTypeTest();
			_test.integer = 1337;
			_test.identifier = "vtTest";
			ValueTypeTest _test2 = new ValueTypeTest();
			_test2.integer = 123;
			_test2.identifier = "vtTest2";
			ValueTypeTestNoRoot _test3 = new ValueTypeTestNoRoot();
			_test3.integer = 1337;

			CollectionTest test = new CollectionTest();
			test.test = new ValueTypeTestNoRoot[] { _test3 };
			test.test2 = new ValueTypeTest[] { _test2, _test }.ToList();
			test.test3 = new List<int>();
			test.test3.Add(321);
			test.test3.Add(132);

			List<ISerializableRoot> roots = new List<ISerializableRoot>();
			roots.Add(test);
			roots.Add(_test2);
			roots.Add(_test);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, null, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 3);
			Assert.AreEqual(test, roots[0]);
			Assert.AreEqual(1337, (roots[0] as CollectionTest).test2[1].integer);
			Assert.AreEqual(132, (roots[0] as CollectionTest).test3[1]);
		}

        [Test]
		public void TestNonSerialized()
		{
			var serializer = CreateSerializer();
			NonSerializedTest test = new NonSerializedTest();
			test.integer = 1337;
			test.integer2 = 13;
			test.integer3 = 999;

			List<ISerializableRoot> roots = new List<ISerializableRoot>();
			roots.Add(test);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, null, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 1);
			Assert.AreEqual(test, roots[0]);
			Assert.AreEqual((roots[0] as NonSerializedTest).integer2, -1);
			Assert.AreEqual((roots[0] as NonSerializedTest).integer3, -1);
		}

        [Test]
		public void TestAlwaysSerialized()
		{
			var serializer = CreateSerializer();
			AlwaysSerializedTest test = new AlwaysSerializedTest();
			test.test = null;
			AlwaysSerializedTest test2 = new AlwaysSerializedTest();
			test2.test = "lul";

			List<ISerializableRoot> roots = new List<ISerializableRoot>();
			roots.Add(test);
			roots.Add(test2);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, null, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 2);
			Assert.AreEqual(test.identifier, roots[0].identifier);
			Assert.AreEqual(test2.identifier, roots[1].identifier);
			Assert.AreEqual(test, roots[0]);
			Assert.AreEqual(test2, roots[1]);
			Assert.AreEqual((roots[0] as AlwaysSerializedTest).test, null);
		}

        [Test]
		public void TestReferences()
		{
			var serializer = CreateSerializer();
			ReferenceTest test = new ReferenceTest();
			test.someStr = "i am object #1";
			test.identifier = "test";
			ReferenceTest test2 = new ReferenceTest();
			test2.someStr = "i am object #2";
			test2.identifier = "test2";
			test2.someRef = test;
			ReferenceTest test3 = new ReferenceTest();
			test3.someStr = "i am object #3";
			test3.identifier = "test2";
			test3.someRef = test;

			List<ISerializableRoot> roots = new List<ISerializableRoot>();
			roots.Add(test);
			roots.Add(test2);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, null, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 2);
			Assert.AreEqual(test.identifier, roots[0].identifier);
			Assert.AreEqual(test2.identifier, roots[1].identifier);
			Assert.AreEqual(test, roots[0]);
			Assert.AreEqual(test2, roots[1]);
		}

        [Test]
		public void TestExternalReferences()
		{
			var serializer = CreateSerializer();
			ReferenceTest test = new ReferenceTest();
			test.someStr = "i am object #1";
			test.identifier = "test";
			ReferenceTest test2 = new ReferenceTest();
			test2.someStr = "i am object #2";
			test2.identifier = "test2";
			test2.someRef = test;

			List<ISerializableRoot> roots = new List<ISerializableRoot>(), externalRefs = new List<ISerializableRoot>();
			roots.Add(test2);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			externalRefs.Add(test);
			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, externalRefs, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 1);
			Assert.AreEqual(test2.identifier, roots[0].identifier);
			Assert.AreEqual(test2, roots[0]);
			Assert.IsTrue(ReferenceEquals(test2.someRef, test));
		}

        [Test]
		public void TestPolymorphism()
		{
			var serializer = CreateSerializer();
			PolymorphismTestRoot test = new PolymorphismTestRoot();
			test.test = new PolymorphismTest1()
			{
				test = 1337
			};
			test.identifier = "test";
			PolymorphismTestRoot test2 = new PolymorphismTestRoot();
			test2.test = new PolymorphismTest2()
			{
				test = 123
			};
			test2.identifier = "test2";

			List<ISerializableRoot> roots = new List<ISerializableRoot>(), externalRefs = new List<ISerializableRoot>();
			roots.Add(test);
			roots.Add(test2);
			string xml = serializer.Serialize(roots, out errors);
			FlushErrors();
			Debug.Log(xml);

			externalRefs.Add(test);
			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, externalRefs, out roots, out errors);
			FlushErrors();
			
			Assert.AreEqual(roots.Count, 2);
			Assert.AreEqual(test.identifier, roots[0].identifier);
			Assert.AreEqual(test2.identifier, roots[1].identifier);
			Assert.AreEqual(test, roots[0]);
			Assert.AreEqual(test2, roots[1]);
		}
    }
#pragma warning restore CS0659
}