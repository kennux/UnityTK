using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEssentials.AssetManagement;

public class AssetExampleUI : MonoBehaviour
{
    public List<AssetExample> assets;

    public string path;
    public string bundleName;
    private AssetBundle assetBundle;

    private void Awake()
    {
        this.assetBundle = AssetBundle.LoadFromFile(this.path + "/" + this.bundleName);

        AssetManager.instance.LoadAssetBundle(this.assetBundle);
        this.assets = AssetManager.instance.GetObjects<AssetExample>("example");
    }

    [ContextMenu("Compile")]
    private void Compile()
    {
#if UNITY_EDITOR
        UnityEditor.BuildPipeline.BuildAssetBundles(this.path, UnityEditor.BuildAssetBundleOptions.None, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#endif
    }
}
