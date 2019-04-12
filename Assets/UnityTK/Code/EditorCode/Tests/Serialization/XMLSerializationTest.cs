using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityTK.Serialization.Prototypes;
using UnityTK.Serialization;

namespace UnityTK.Test.Serialization
{
	public class XMLSerializationTest : MonoBehaviour
	{
		class TestRoot : ISerializableRoot
		{
			public string identifier { get; set; }
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
		public void TestValueTypeSerialization()
		{
			var serializer = CreateSerializer();
			ValueTypeTest test = new ValueTypeTest();
			test.integer = 1337;
			test.identifier = "test";

			List<ISerializableRoot> roots = new List<ISerializableRoot>(), tmp = new List<ISerializableRoot>();
			roots.Add(test);
			string xml = serializer.Serialize(roots, tmp, out errors);
			FlushErrors();
			Debug.Log(xml);

			roots.Clear();
			serializer.Deserialize(new string[] { xml }, new string[] { "MEMORY" }, out roots, out errors);
			FlushErrors();

			Assert.AreEqual(roots.Count, 1);
			Assert.AreEqual(test, roots[0]);
		}
	}
}