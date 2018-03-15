using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Main asset management class.
    /// Provides high-level api to working with UnityTK's asset management system at runtime.
    /// 
    /// This system provides the following functionality:
    /// - Loading and unloading asset bundles at runtime
    /// - Indexing the loaded asset bundle assets via <see cref="ManagedScriptableObject"/>
    /// - Easy runtime access to the loaded assets via a flexible api
    /// 
    /// When the asset manager loads an asset bundle, it will read all <see cref="ManagedScriptableObject"/> and GameObjects.
    /// The managed scriptable objects will directly be registered as is.
    /// The GameObjects will be queried for a component implementing <see cref="IManagedAsset"/> (<see cref="ManagedGameObject"/>).
    /// Every implementation found is registered to the asset manager.
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        #region Singleton
        /// <summary>
        /// Asset manager singleton, will create a new gameobject and DontDestroyOnLoad's it.
        /// </summary>
        public static AssetManager instance
        {
            get
            {
                if (Essentials.UnityIsNull(_instance))
                {
                    var go = new GameObject("_AssetManager_");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AssetManager>();
                }

                return _instance;
            }
        }
        private static AssetManager _instance;
        #endregion

        /// <summary>
        /// All assets registered to the manager.
        /// </summary>
        private HashSet<IManagedAsset> registeredAssets = new HashSet<IManagedAsset>();
        private List<IManagedAsset> _registeredAssets = new List<IManagedAsset>();

        #region Asset registration

        /// <summary>
        /// Registers the specified asset identifier to this asset manager.
        /// </summary>
        public void RegisterAsset(IManagedAsset asset)
        {
            if (registeredAssets.Contains(asset))
                return;

            Debug.Log("Registered asset " + asset);
            registeredAssets.Add(asset);
            _registeredAssets.Add(asset);
        }

        /// <summary>
        /// Deregisters the specified asset from this asset manager.
        /// Must have previously been registered by either asset bundle loading or <see cref="RegisterAsset(IManagedAsset)"/>
        /// </summary>
        public void DeregisterAsset(IManagedAsset asset)
        {
            if (!registeredAssets.Contains(asset))
                return;

            _registeredAssets.Remove(asset);
            registeredAssets.Remove(asset);
        }

        #endregion

        #region Query logic

        /// <summary>
        /// Queries the asset manager for assets.
        /// </summary>
        /// <typeparam name="T">The type the objects must have to end up in the result set. Scriptable objects are being checked if they are assignable to the specified type.
        /// GameObjects will be checked whether or not they have a component of the specified type.
        /// If T is GameObject, the IManagedAsset implementation will be casted to Component to retrieve the gameobject.</typeparam>
        /// <param name="query">The query parameters</param>
        /// <param name="preAlloc">A pre-allocated list that will be used as return list. If not supplied, a list from the <see cref="ListPool{T}"/> is being drawn. This list can also already be containing objects.</param>
        /// <param name="limit">The limit of how many objects to look for maximum. -1 employs no limit.</param>
        /// <param name="throwCastException">whether or not an <see cref="System.InvalidCastException"/> will be thrown if the type T cannot be retrieved from an asset that has the specified tag.</param>
        public List<T> Query<T>(IAssetManagerQuery query, List<T> preAlloc = null, int limit = -1, bool throwCastException = false) where T : UnityEngine.Object
        {
            ListPool<T>.GetIfNull(ref preAlloc);

            int selected = 0;
            for (int i = 0; i < _registeredAssets.Count; i++)
            {
                // Read asset and cast
                var asset = _registeredAssets[i];
                if (!query.MatchesCriterias(asset))
                    continue;

                var casted = asset.GetAs<T>();

                // Was casting not successfull?
                if (Essentials.UnityIsNull(casted))
                {
                    if (throwCastException)
                        throw new System.InvalidCastException("Object " + asset.name + " wasnt castable to " + typeof(T));
                }
                else
                {
                    preAlloc.Add(casted);

                    if (limit != -1 && selected >= limit)
                        break;
                }
            }

            return preAlloc;
        }

        #endregion
    }
}