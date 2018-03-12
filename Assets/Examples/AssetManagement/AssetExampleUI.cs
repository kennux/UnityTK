using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEssentials.AssetManagement;

public class AssetExampleUI : MonoBehaviour
{
    public List<AssetExample> assets;

    public void Start()
    {
        this.assets = AssetManager.instance.GetObjects<AssetExample>("example");
    }
}
