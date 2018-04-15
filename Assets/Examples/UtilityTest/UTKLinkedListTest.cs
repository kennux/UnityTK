using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

public class UTKLinkedListTest : MonoBehaviour
{
    private UTKLinkedList<int> list = new UTKLinkedList<int>();

	// Use this for initialization
	void Start ()
    {
        for (int i = 0; i < 10; i++)
            this.list.Add(i);
	}
	
	// Update is called once per frame
	void Update ()
    {
        int j = 0;
        // Test foreach GC alloc
        UnityEngine.Profiling.Profiler.BeginSample("Foreach");
        foreach (int num in this.list)
        {
            j += num;
        }
        UnityEngine.Profiling.Profiler.EndSample();

        Debug.Assert(j == (1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9));

        UnityEngine.Profiling.Profiler.BeginSample("Recreation");
        // Test recreation GC alloc
        this.list.Clear();
        for (int i = 0; i < 10; i++)
            this.list.Add(i);
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
