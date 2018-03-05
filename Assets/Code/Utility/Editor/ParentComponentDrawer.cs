using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityEssentials.Editor
{
    /// <summary>
    /// Property drawer implementation for <see cref="ParentComponentAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ParentComponentAttribute))]
    public class ParentComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Multi-editing isnt supportedd
            if (property.serializedObject.isEditingMultipleObjects)
                return;

            // Type check
            var self = property.serializedObject.targetObject;
            var value = property.objectReferenceValue;
            if (!(self is Component))
            {
                Debug.LogError("Parent component attribute isnt allowed on non-component classes :(");
                return;
            }

            // Gather info
            var attrib = this.attribute as ParentComponentAttribute;
            var valueComponent = value as Component;
            var selfComponent = self as Component;
            var availableComponents = selfComponent.GetComponentsInParent(attrib.targetType).Where((c) => !object.ReferenceEquals(self, c)).ToArray();
            var availableComponentsStr = new string[] { "NULL", }.Concat(availableComponents.Select((c) => c.ToString())).ToArray();
            var currentlySelected = property.objectReferenceValue;
            int currentlySelectedIndex = System.Array.IndexOf(availableComponents, valueComponent) + 1;

            int newIndex = EditorGUI.Popup(position, property.name, currentlySelectedIndex, availableComponentsStr);
            if (newIndex != currentlySelectedIndex)
            {
                if (newIndex == 0)
                    property.objectReferenceValue = null;
                else
                    property.objectReferenceValue = availableComponents[newIndex-1];
            }
        }
    }
}