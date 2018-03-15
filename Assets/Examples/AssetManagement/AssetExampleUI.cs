using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.AssetManagement;

public class AssetExampleUI : MonoBehaviour
{
    public List<AssetExample> assets;
    public List<AssetExample2> assets2;

    public void Start()
    {
        var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
        query.AddTagCriteria("example");

        this.assets = AssetManager.instance.Query<AssetExample>(query);
        this.assets2 = AssetManager.instance.Query<AssetExample2>(query);

        AssetManagerQueryPool<AssetManagerQuery>.Return(query);
    }
}
