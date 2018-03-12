using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials.AssetManagement
{
    /// <summary>
    /// Base class that can be used for implementing manageable scriptable objects.
    /// <see cref="IManagedAsset"/>, <see cref="AssetManagement"/>
    /// </summary>
    public abstract class ManagedScriptableObject : ScriptableObject, IManagedAsset
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
            return this as T;
        }
    }
}