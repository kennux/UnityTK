using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK
{
    /// <summary>
    /// Datastructure for storing instances of a mesh.
    /// Used on <see cref="StaticBatching"/>
    /// </summary>
    internal struct StaticMeshInstance
    {
        /// <summary>
        /// The owner of this mesh instance.
        /// </summary>
        public object owner;

        /// <summary>
        /// The transform (TRS) matrix.
        /// </summary>
        public Matrix4x4 transform;

        /// <summary>
        /// The group key of the instance.
        /// </summary>
        public StaticBatchingGroupKey groupKey;

        /// <summary>
        /// The visual representation of the mesh instance.
        /// </summary>
        public GameObject visualRepresentation;
    }
}
