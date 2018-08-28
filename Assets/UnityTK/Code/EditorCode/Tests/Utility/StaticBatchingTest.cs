using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityTK.Test
{
    public class StaticBatchingTest
    {
        private static Mesh GenerateMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] indices, MeshTopology topo = MeshTopology.Points)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            return mesh;
        }

        /// <summary>
        /// Tests simply batching 2 meshes with same layer, material and worldspace chunk.
        /// </summary>
        [Test]
        public void TestSimpleBatching()
        {
            var m1 = GenerateMesh(new Vector3[] { Vector3.one, Vector3.one * 2f }, new Vector3[] { Vector3.up, Vector3.up }, new Vector2[] { Vector2.up, Vector2.up }, new int[] { 0, 1 }, MeshTopology.Points);
            var m2 = GenerateMesh(new Vector3[] { Vector3.one * 3f, Vector3.one * 4f }, new Vector3[] { Vector3.down, Vector3.down }, new Vector2[] { Vector2.down, Vector2.down }, new int[] { 0, 1 }, MeshTopology.Points);
            Material mat = new Material(Shader.Find("Standard"));
            object owner = new object();

            StaticBatching.instance.InsertMesh(m1, mat, 8, Matrix4x4.identity, owner);
            StaticBatching.instance.InsertMesh(m1, mat, 8, Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one), owner);
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