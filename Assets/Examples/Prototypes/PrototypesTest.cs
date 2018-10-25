using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityTK.Prototypes;

public class PrototypesTest : MonoBehaviour
{
	public TextAsset xml;

	public void Start()
	{
		List<ParsingError> parsingErrors = new List<ParsingError>();
		var objs = Prototypes.Parse(this.xml.text, new PrototypeParseParameters()
		{
			standardNamespace = "UnityTK.Examples.Prototypes"
		}, ref parsingErrors);

		if (parsingErrors.Count == 0)
		{
			Debug.Log("Parsed successfully " + objs.Count + " objects!");
		}
		else
		{
			foreach (var error in parsingErrors)
				error.DoUnityDebugLog();
		}
	}
}