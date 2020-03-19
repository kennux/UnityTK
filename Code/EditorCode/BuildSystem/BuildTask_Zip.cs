using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace UnityTK.BuildSystem
{
	/// <summary>
	/// <see cref="BuildTask"/> which will run a job.
	/// </summary>
	[CreateAssetMenu(fileName = "CreateZipTask", menuName = "UnityTK/BuildSystem/Create Zip Task")]
	public class BuildTask_Zip : BuildTask
	{
		[Header("Build config")]
		public List<string> subfolders;
		public string archiveDestination;

		[Header("Backend: 7-Zip")]
		public string backend7ZipExePath = "C:/Program Files/7-Zip/7z.exe";

		public override void Run(BuildJob job, BuildJobParameters parameters)
		{
			List<string> subfolders = new List<string>(this.subfolders);
			if (subfolders.Count == 1)
				subfolders[0] += "/*";

			if (File.Exists(backend7ZipExePath))
			{
				// Use 7zip backend
				string arguments = "a " + Path.Combine(parameters.destination, archiveDestination) + " " + string.Join(" ", subfolders.Select(s => "\"" + Path.Combine(parameters.destination, s) + "\""));
				Process.Start(backend7ZipExePath, arguments).WaitForExit();
			}
		}
	}
}