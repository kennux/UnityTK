using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEssentials.BuildSystem
{
    /// <summary>
    /// Build system unity editor window implementation.
    /// </summary>
    public class BuildSystem : EditorWindow
    {
        #region Singleton
        private static BuildSystem instance;
        [MenuItem("UnityEssentials/Build System")]
        static void Init()
        {
            if (Essentials.UnityIsNull(instance))
                instance = GetWindow<BuildSystem>();
            instance.Show();
        }
        #endregion

        private string destination;
        private bool deleteExistingDestination;
        private BuildJob job;

        public void OnEnable()
        {
            this.titleContent = new GUIContent("Build System");
        }

        public void OnGUI()
        {
            // Destination settings
            this.destination = EditorGUILayout.TextField("Destination", this.destination);
            if (GUILayout.Button("Select Path"))
            {
                this.destination = EditorUtility.OpenFolderPanel("Select build destination", this.destination, "Build");
            }

            this.deleteExistingDestination = EditorGUILayout.Toggle("Clear destination", this.deleteExistingDestination);

            // Job settings
            this.job = EditorGUILayout.ObjectField(this.job, typeof(BuildJob), false) as BuildJob;
            if (GUILayout.Button("Build Job"))
            {
                this.job.Run(new BuildJobParameters(this.destination, this.deleteExistingDestination));
            }
        }
    }
}