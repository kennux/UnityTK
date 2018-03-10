using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityEssentials.DataBinding.Editor.Test
{
    public class DataBindingRootsTest
    {
        /// <summary>
        /// Creates a databinding root binding to an instance of <see cref="DataBindingTest"/>.
        /// </summary>
        public static DataBindingRoot CreateRootWithTest(out DataBindingExample testBindTarget)
        {
            var rootGo = new GameObject("Root");
            testBindTarget = rootGo.AddComponent<DataBindingExample>();
            var root = rootGo.AddComponent<DataBindingRoot>();
            root.target = testBindTarget;
            root.Awake();

            return root;
        }

        [Test]
        public void DataBindingRootTest()
        {
            // Create root
            DataBindingExample example;
            var root = CreateRootWithTest(out example);

            Assert.AreEqual(example, root.boundObject);
            Assert.AreEqual(typeof(DataBindingExample), root.boundType);
        }
    }
}