using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class StaticBatchingTest
    {
        /// <summary>
        /// Tests simply batching 2 meshes with same layer, material and worldspace chunk.
        /// </summary>
        [Test]
        public void TestSimpleBatching()
        {
        }

        /// <summary>
        /// Test batching 2 meshes together with different materials.
        /// </summary>
        [Test]
        public void TestMultiMaterialBatching()
        {
        }

        /// <summary>
        /// Test batching 2 meshes together with different worldspace chunks.
        /// </summary>
        [Test]
        public void TestChunkedBatching()
        {
        }

        /// <summary>
        /// Test batching 2 meshes together with different gameobject layers.
        /// </summary>
        [Test]
        public void TestMultiLayerBatching()
        {
        }
    }
}