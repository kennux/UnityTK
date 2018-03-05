using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials
{
    /// <summary>
    /// Parent component property attribute that can be used to annotate fields which are being selected from a set of components gathered from the gameobject parents.
    /// </summary>
    public class ParentComponentAttribute : PropertyAttribute
    {
        public System.Type targetType;

        public ParentComponentAttribute(System.Type targetType)
        {
            this.targetType = targetType;
        }
    }
}