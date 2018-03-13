using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEssentials.AssetManagement;

public class AssetExampleUI : MonoBehaviour
{
    public List<AssetExample> assets;
    public List<AssetExample2> assets2;

    public void Start()
    {
        this.assets = AssetManager.instance.GetObjects<AssetExample>("example");
        this.assets2 = AssetManager.instance.GetObjects<AssetExample2>("example");
    }
}
