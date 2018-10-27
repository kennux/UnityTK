using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityTK.Prototypes;
using UnityTK.Examples.Prototypes;

public class PrototypesTest : MonoBehaviour
{
	public TextAsset xml;
	public TestPrototype[] loadedPrototypes;

	public void Start()
	{
		var parser = new PrototypeParser();
		parser.Parse(this.xml.text, "DIRECT PARSE", new PrototypeParseParameters()
		{
			standardNamespace = "UnityTK.Examples.Prototypes"
		});

		var errors = parser.GetParsingErrors();
		this.loadedPrototypes = parser.GetPrototypes().Cast<TestPrototype>().ToArray();

		if (errors.Count == 0)
		{
			Debug.Log("Parsed successfully " + loadedPrototypes.Length+ " objects!");
		}
		else
		{
			foreach (var error in errors)
				error.DoUnityDebugLog();
		}
	}
}