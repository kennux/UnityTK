using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityEssentials.AssetManagement
{
    /// <summary>
    /// Simple asset bundle loader implementation that can be used to load bundles to <see cref="AssetManager.LoadAssetBundle(AssetBundle)"/>.
    /// This will load all files in the specified folder as asset bundles.
    /// Will filter for files without or .bundle extension.
    /// 
    /// In the editor this loader will simulate asset bundle loading by registering all assets in any bundle.
    /// <see cref="AssetManager.EditorLoadAndRegisterAssetsInBundles"/>
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
        /// <summary>
        /// The path from where asset bundles are being loaded at runtime (in player).
        /// </summary>
        public string loadPath;

        public void Awake()
        {
#if UNITY_EDITOR
            AssetManager.instance.EditorLoadAndRegisterAssetsInBundles();
#else
            foreach (var file in Directory.GetFiles(loadPath))
            {
                string ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext) || ext.Equals(".bundle"))
                {
                    Debug.Log("Loading Asset Bundle " + file);
                    AssetManager.instance.LoadAssetBundle(AssetBundle.LoadFromFile(file));
                }
            }
#endif
        }
    }
}
