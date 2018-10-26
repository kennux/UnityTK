using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.Prototypes;
using System.Linq;

namespace UnityTK.AssetManagement
{
	public class PrototypesLoader : AssetLoader
	{
		public const string StreamingAssetsToken = "[STREAMING_ASSETS]";

		public string path = StreamingAssetsToken;
		public string standardNamespace = "";
		
		protected override List<IManagedAsset> LoadAssets()
		{
			string path = this.path.Replace(StreamingAssetsToken, Application.streamingAssetsPath);
			
			List<ParsingError> parsingErrors = new List<ParsingError>();

			// Load data
			var files = System.IO.Directory.GetFiles(path, "*.xml", System.IO.SearchOption.AllDirectories);
			string[] contents = files.Select((f) => System.IO.File.ReadAllText(f)).ToArray();
			var prototypes = PrototypeParser.Parse(contents, files, new PrototypeParseParameters()
			{
				standardNamespace = this.standardNamespace
			}, ref parsingErrors);

			foreach (var error in parsingErrors)
				error.DoUnityDebugLog();

			return prototypes.Where((p) => p is IManagedPrototype).Cast<IManagedAsset>().ToList();
		}

		protected override void UnloadAssets(List<IManagedAsset> assets)
		{
		}
	}
}