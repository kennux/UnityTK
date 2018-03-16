using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Implements visual representation behaviour that can be used to render a gameobject as a visual representation of a gameobject.
    /// 
    /// In order to render the assigned visual representation, the gameobject is scanned for every <see cref="MeshRenderer"/> and a cache is generated.
    /// When the representation is being rendered, it will render every mesh found in the previous scan step relatively to the gameobject (of the representation) transformation.
    /// 
    /// This comes in very handy if you want to render loads of objects very efficiently or when trying to implement game data where modders can easily change visualization, ...
    /// </summary>
    public class VisualRepresentation : MonoBehaviour
    {

    }
}