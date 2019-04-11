using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Serialization
{
    public interface ISerializableRoot
    {
		/// <summary>
		/// The identifier of the serializable object.
		/// This identifier is the one to be used for referencing objects from other data.
		/// </summary>
		string identifier { get; set; }
    }
}
