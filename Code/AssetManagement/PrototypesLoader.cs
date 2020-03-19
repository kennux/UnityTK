using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.Serialization.Prototypes;
using System.Linq;
using UnityTK.Serialization;

namespace UnityTK.AssetManagement
{
	public class PrototypesLoader : AssetLoader
	{
		public const string StreamingAssetsToken = "[STREAMING_ASSETS]";

		public string[] paths = new string[] { StreamingAssetsToken };
		public string standardNamespace = "";
		
		protected override List<IManagedAsset> LoadAssets()
		{
			var parser = new PrototypeParser(PrototypeParser.CreateXMLSerializer(standardNamespace));
			foreach (var p in this.paths)
			{
				string path = p.Replace(StreamingAssetsToken, Application.streamingAssetsPath);

				// Load data
				var files = System.IO.Directory.GetFiles(path, "*.xml", System.IO.SearchOption.AllDirectories);
				string[] contents = files.Select((f) => System.IO.File.ReadAllText(f)).ToArray();
				parser.Parse(contents, files);
			}

			var prototypes = parser.GetPrototypes();
			var errors = parser.GetParsingErrors();

			foreach (var error in errors)
				error.DoUnityDebugLog();

			return prototypes.Where((p) => p is IManagedPrototype).Cast<IManagedAsset>().ToList();
		}

		protected override void UnloadAssets(List<IManagedAsset> assets)
		{
		}
	}
}