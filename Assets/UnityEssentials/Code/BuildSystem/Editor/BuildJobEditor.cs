using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEssentials.BuildSystem
{
    /// <summary>
    /// Custom inspector implementation for <see cref="BuildJob"/>
    /// </summary>
    [CustomEditor(typeof(BuildJob))]
    public class BuildJobEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (this.serializedObject.isEditingMultipleObjects)
                return;

            var tasksProp = this.serializedObject.FindProperty("tasks");
            EditorGUILayout.PropertyField(tasksProp, true);

            // Draw tasks
            var tasks = ((BuildJob)this.target).tasks;
            for (int i = 0; i < tasks.Length; i++)
            {
                if (Essentials.UnityIsNull(tasks[i]))
                    continue;

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(tasks[i].ToString(), EditorStyles.boldLabel);
                EditorGUILayout.Space();

                CreateEditor(tasks[i]).OnInspectorGUI();
            }
        }
    }
}