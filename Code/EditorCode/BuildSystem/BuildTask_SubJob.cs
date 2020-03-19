using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityTK.BuildSystem
{
	/// <summary>
	/// <see cref="BuildTask"/> which will run a job.
	/// </summary>
	[CreateAssetMenu(fileName = "SubJobTask", menuName = "UnityTK/BuildSystem/Run SubJob Task")]
	public class BuildTask_SubJob : BuildTask
	{
		[Header("Task")]
		public string subfolder;

		[Header("Build config")]
		public BuildJob subJob;

		public override void Run(BuildJob job, BuildJobParameters parameters)
		{
			string dst;
			if (!string.IsNullOrWhiteSpace(subJob.destination))
				dst = Path.Combine(parameters.destination, subJob.destination);
			else
				dst = parameters.destination;

			subJob.Run(new BuildJobParameters(dst, parameters.deleteExistingDestination));
		}
	}
}