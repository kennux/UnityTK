using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityEssentials
{
    /// <summary>
    /// Helper class for procedural mesh generation.
    /// This class can take several mesh generation commands and then concat them to a final mesh.
    /// </summary>
    public class ProceduralMeshGenerator
    {
        private static int[] meshTopologyVerticesPerFace = new int[]
        {
            3,
            0,
            4,
            2,
            1,
            1
        };

        public class Face
        {
            public Vector3[] positions;
            public Vector3[] normals;
            public Vector2[] uvs;
            public short[] indices;
        }

        private List<Face> faces;
        private MeshTopology topology;

        public ProceduralMeshGenerator(MeshTopology topology)
        {
            this.faces = new List<Face>();
            this.topology = topology;
        }

        public void AddFace(Vector3[] positions, Vector3[] normals, Vector2[] uvs, int[] indices)
        {
            AddFace(positions, normals, uvs, System.Array.ConvertAll(indices, (i) => (short)i));
        }

        public void AddFace(Vector3[] positions, Vector3[] normals, Vector2[] uvs, short[] indices)
        {
            this.faces.Add(new Face()
            {
                indices = indices,
                normals = normals,
                uvs = uvs,
                positions = positions
            });
        }

        /// <summary>
        /// Creates the mesh object.
        /// </summary>
        /// <returns></returns>
        public Mesh CreateMesh()
        {
            // Build mesh
            // TODO: OPTIMIZE THIS!
            // TODO: Implement mesh splitting

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            int indexCounter = 0;
            int verticesPerFace = meshTopologyVerticesPerFace[(int)this.topology];
            Bounds bounds = new Bounds();

            foreach (var face in this.faces)
            {
                positions.AddRange(face.indices.Select((i) => face.positions[i]));
                normals.AddRange(face.indices.Select((i) => face.normals[i]));
                uvs.AddRange(face.indices.Select((i) => face.uvs[i]));

                for (int i = 0; i < verticesPerFace; i++)
                {
                    bounds.Encapsulate(face.positions[face.indices[i]]);
                    indices.Add(indexCounter);
                    indexCounter++;
                }
            }

            Mesh m = new Mesh();
            m.SetVertices(positions);
            m.SetNormals(normals);
            m.SetUVs(0, uvs);
            m.SetIndices(indices.ToArray(), this.topology, 0);
            m.bounds = bounds;

            return m;
        }
    }
}