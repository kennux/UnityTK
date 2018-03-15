using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Implements the managed asset interface as monobehaviour component.
    /// <see cref="IManagedAsset"/>
    /// </summary>
    public class ManagedGameObject : MonoBehaviour, IManagedAsset
    {
        /// <summary>
        /// The tags this asset has assigned.
        /// </summary>
        public string[] tags;

        string[] IManagedAsset.tags
        {
            get
            {
                return this.tags;
            }
        }

        public T GetAs<T>() where T : Object
        {
            if (!typeof(Component).IsAssignableFrom(typeof(T)))
                return null;

            return this.GetComponent<T>();
        }
    }
}
