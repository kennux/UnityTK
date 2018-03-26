using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

public class ReusableBoxTest : MonoBehaviour
{
    public void Update()
    {
        // Test continously for GC alloc
        ReusableBox<int> box = ReusableBox<int>.Box(123);
        box.Equals(123);
        box.GetHashCode();
        box.Dispose();
    }
}
