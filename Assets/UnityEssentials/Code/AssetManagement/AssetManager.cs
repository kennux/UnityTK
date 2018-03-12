using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials.AssetManagement
{
    /// <summary>
    /// Main asset management class.
    /// Provides high-level api to working with UnityEssentials's asset management system at runtime.
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

        /// <summary>
        /// Maps an <see cref="IManagedAsset.tags"/> to a list of asset references.
        /// </summary>
        private Dictionary<string, List<IManagedAsset>> assetsMap = new Dictionary<string, List<IManagedAsset>>();

        /// <summary>
        /// Maps an asset bundle to a list of all assets loaded from it.
        /// </summary>
        private Dictionary<AssetBundle, List<IManagedAsset>> assetBundleMap = new Dictionary<AssetBundle, List<IManagedAsset>>();

        #region Load / Register logic

#if UNITY_EDITOR

        /// <summary>
        /// Loads all assets assigned to an asset bundle in the editor and registers it if it has an <see cref="IManagedAsset"/> implementation.
        /// This can be used to simulate asset bundles in the editor (in use in <see cref="AssetBundleLoader"/>
        /// </summary>
        public void EditorLoadAndRegisterAssetsInBundles()
        {
            foreach (var bundle in UnityEditor.AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var path in UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(path);

                    // Scriptable object / gameobject registering
                    if (obj is IManagedAsset)
                        this.RegisterAsset((IManagedAsset)obj);
                    else if (obj is GameObject)
                    {
                        var ma = (obj as GameObject).GetComponent<IManagedAsset>();
                        if (!Essentials.UnityIsNull(ma))
                            this.RegisterAsset(ma);
                    }
                }
            }
        }

#endif

        /// <summary>
        /// Loads the specified asset bundle.
        /// </summary>
        public void LoadAssetBundle(AssetBundle bundle)
        {
            if (this.assetBundleMap.ContainsKey(bundle))
                return;

            // Load assets
            var assets = new List<IManagedAsset>(bundle.LoadAllAssets<ManagedScriptableObject>());
            var gos = bundle.LoadAllAssets<GameObject>();
            for (int i = 0; i < gos.Length; i++)
            {
                // Get and validate gameobject
                var go = gos[i];
                if (Essentials.UnityIsNull(go))
                {
                    Debug.LogWarning("Asset bundle contained a gameobject that failed null equality check!");
                    continue;
                }

                // Try getting the component
                var managedAsset = go.GetComponent<IManagedAsset>();
                if (Essentials.UnityIsNull(managedAsset))
                    continue;

                assets.Add(managedAsset);

            }

            // Register bundle as loaded
            this.assetBundleMap.Add(bundle, assets);

            // Register assets
            for (int i = 0; i < assets.Count; i++)
                RegisterAsset(assets[i]);
        }

        /// <summary>
        /// Registers the specified asset identifier to this asset manager.
        /// </summary>
        public void RegisterAsset(IManagedAsset asset)
        {
            if (registeredAssets.Contains(asset))
                return;

            List<IManagedAsset> lst;
            foreach (var tag in asset.tags)
            {
                if (!assetsMap.TryGetValue(tag, out lst))
                {
                    lst = new List<IManagedAsset>();
                    assetsMap.Add(tag, lst);
                }
                lst.Add(asset);
            }

            Debug.Log("Registered asset " + asset);
            registeredAssets.Add(asset);
        }

        /// <summary>
        /// Deregisters the specified asset from this asset manager.
        /// Must have previously been registered by either asset bundle loading or <see cref="RegisterAsset(IManagedAsset)"/>
        /// </summary>
        public void DeregisterAsset(IManagedAsset asset)
        {
            if (!registeredAssets.Contains(asset))
                return;

            List<IManagedAsset> lst;
            foreach (var tag in asset.tags)
            {
                if (assetsMap.TryGetValue(tag, out lst))
                    lst.Remove(asset);
            }

            registeredAssets.Remove(asset);
        }

        /// <summary>
        /// Unloads the specified asset bundle.
        /// 
        /// This method only <see cref="AssetBundle.Unload(bool)"/>s the bundle!
        /// If you wish to completely unload it from memory, you need to Destroy the asset bundle via the unity api.
        /// </summary>
        public void UnloadAssetBundle(AssetBundle bundle)
        {
            List<IManagedAsset> lst;
            if (!this.assetBundleMap.TryGetValue(bundle, out lst))
                return;

            // Deregister assets
            for (int i = 0; i < lst.Count; i++)
                DeregisterAsset(lst[i]);

            // Remove bundle from loaded bundles
            this.assetBundleMap.Remove(bundle);

            // Unload the bundle
            bundle.Unload(true);
        }

        #endregion
        #region Query logic

        /// <summary>
        /// Returns all objects with the specified tag.
        /// </summary>
        /// <typeparam name="T">The type the objects must have to end up in the result set. Scriptable objects are being checked if they are assignable to the specified type.
        /// GameObjects will be checked whether or not they have a component of the specified type.
        /// If T is GameObject, the IManagedAsset implementation will be casted to Component to retrieve the gameobject.</typeparam>
        /// <param name="preAlloc">A pre-allocated list that will be used as return list. If not supplied, a list from the <see cref="ListPool{T}"/> is being drawn. This list can also already be containing objects.</param>
        /// <param name="throwCastException">whether or not an <see cref="System.InvalidCastException"/> will be thrown if the type T cannot be retrieved from an asset that has the specified tag.</param>
        public List<T> GetObjects<T>(string tag, List<T> preAlloc = null, bool throwCastException = false) where T : Object
        {
            ListPool<T>.GetIfNull(ref preAlloc);

            List<IManagedAsset> lst;
            if (this.assetsMap.TryGetValue(tag, out lst))
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    // Read asset and cast
                    var asset = lst[i];
                    var casted = asset.GetAs<T>();

                    // Was casting not successfull?
                    if (Essentials.UnityIsNull(casted))
                    {
                        if (throwCastException)
                            throw new System.InvalidCastException("Object " + asset.name + " wasnt castable to " + typeof(T));
                    }
                    else
                        preAlloc.Add(casted);
                }
            }

            return preAlloc;
        }

        #endregion
    }
}