using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.AssetManagement;
using UnityTK.Serialization;

[CreateAssetMenu(fileName = "AssetExample", menuName = "UnityTK/Examples/AssetExample")]
public class AssetExample : ManagedScriptableObject, IManagedPrototype
{
    public string testStr;

	string ISerializableRoot.identifier
	{
		get
		{
			return this.identifier;
		}

		set
		{
			this.identifier = value;
		}
	}
}
