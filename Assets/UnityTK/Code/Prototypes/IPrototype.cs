using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// Prototype interface that can be used to implement prototypes loadable by <see cref="Prototypes"/> from XML.
	/// </summary>
	public interface IPrototype
	{
		/// <summary>
		/// The name of the prototype.
		/// This name is the one to be used for referencing prototypes in other data.
		/// 
		/// It is automatically written by the parsers using the XML attribute "Name" on the prototype XMLElement.
		/// </summary>
		string name { get; set; }
	}
}