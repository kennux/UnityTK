using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.AssetManagement;
using UnityTK.Prototypes;

[CreateAssetMenu(fileName = "AssetExample", menuName = "UnityTK/Examples/AssetExample")]
public class AssetExample : ManagedScriptableObject, IManagedPrototype
{
    public string testStr;

	string IPrototype.identifier
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
