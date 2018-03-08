using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBindingExample : MonoBehaviour
{
    public string testStr;
    public Nest nest;

    public List<Nest> nestArray;

    [System.Serializable]
    public class Nest
    {
        public string testStr;
    }

    public string Test()
    {
        this.testStr = Random.value.ToString();
        return this.testStr;
    }

    public string Test2()
    {
        return Random.value.ToString();
    }

    public void Test3(string test)
    {
        Debug.Log(test);
    }
}
