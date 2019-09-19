using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Serialization
{
	public interface ISerializer
	{
		void Deserialize(string[] data, string[] filenames, List<ISerializableRoot> externalReferenceables, out List<ISerializableRoot> parsedObjects, out List<SerializerError> errors);
		string Serialize(List<ISerializableRoot> roots, out List<SerializerError> errors);
	}

	public abstract class SerializerBase<T> : ISerializer
	{
		protected T parameters;

		public SerializerBase(T parameters)
		{
			this.parameters = parameters;
		}

		public abstract void Deserialize(string[] data, string[] filenames, List<ISerializableRoot> externalReferenceables, out List<ISerializableRoot> parsedObjects, out List<SerializerError> errors);
		public abstract string Serialize(List<ISerializableRoot> roots, out List<SerializerError> errors);
	}

}