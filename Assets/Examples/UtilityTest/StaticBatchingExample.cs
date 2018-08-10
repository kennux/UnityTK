using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

public class StaticBatchingExample : MonoBehaviour
{
    public GameObject prefab;
    public GameObject staticPrefab;
    public Test test;

    public int x = 100;
    public int y = 100;

    public enum Test
    {
        Direct,
        StaticBatchdPrefab,
        StaticBatched
    }

    private void Start()
    {
        for (int x = 0; x < this.x; x++)
            for (int y = 0; y < this.y; y++)
            {
                Spawn(new Vector3(x * 2, 0, y * 2));
            }
    }

    private void Spawn(Vector3 worldspace)
    {
        switch (this.test)
        {
            case Test.Direct: Instantiate(this.prefab, worldspace, Quaternion.identity); break;
            case Test.StaticBatched: StaticBatching.instance.InsertVisualRepresentation(this.prefab, Matrix4x4.TRS(worldspace, Quaternion.identity, Vector3.one), this); break;
            case Test.StaticBatchdPrefab: Instantiate(this.staticPrefab, worldspace, Quaternion.identity); break;
        }
    }
}
