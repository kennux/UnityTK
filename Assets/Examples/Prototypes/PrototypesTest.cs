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
		List<ParsingError> parsingErrors = new List<ParsingError>();
		this.loadedPrototypes = PrototypeParser.Parse(this.xml.text, new PrototypeParseParameters()
		{
			standardNamespace = "UnityTK.Examples.Prototypes"
		}, ref parsingErrors).Cast<TestPrototype>().ToArray();

		if (parsingErrors.Count == 0)
		{
			Debug.Log("Parsed successfully " + loadedPrototypes.Length+ " objects!");
		}
		else
		{
			foreach (var error in parsingErrors)
				error.DoUnityDebugLog();
		}
	}
}