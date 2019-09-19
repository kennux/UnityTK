using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using UnityTK.Serialization;

namespace UnityTK.Serialization.Prototypes
{
	/// <summary>
	/// Parser object with the ability to parse prototypes using serializers <see cref="ISerializer"/>.
	/// 
	/// This is an example application of the UnityTK serializers code.
	/// </summary>
	public class PrototypeParser
	{
		public const string RootContainerName = "PrototypeContainer";

		public static XMLSerializer CreateXMLSerializer(string standardNamespace)
		{
			return new XMLSerializer(new XMLSerializerParams()
			{
				rootElementName = RootContainerName,
				standardNamespace = standardNamespace
			});
		}

		private List<SerializerError> errors = new List<SerializerError>();
		private List<IPrototype> prototypes = new List<IPrototype>();
		private List<ISerializableRoot> allRoots = new List<ISerializableRoot>();
		private ISerializer serializer;

		public PrototypeParser(ISerializer serializer)
		{
			this.serializer = serializer;
		}

		/// <summary>
		/// Returns the internal prototypes list.
		/// </summary>
		public List<IPrototype> GetPrototypes()
		{
			return this.prototypes;
		}

		/// <summary>
		/// Returns the internal errors list.
		/// </summary>
		public List<SerializerError> GetParsingErrors()
		{
			return this.errors;
		}

		/// <summary>
		/// Parses the specified data and returns all prototypes which could be parsed.
		/// <see cref="ISerializer.Deserialize(string[], string[], out List{ISerializableRoot})"/>
		/// </summary>
		/// <param name="data">The data to use for parsing.</param>
		/// <param name="filename">The filename used to report errors</param>
		/// <returns></returns>
		public void Parse(string data, string filename, List<ISerializableRoot> externalReferenceables = null)
		{
			List<ISerializableRoot> allReferenceables = allRoots;
			if (externalReferenceables != null && externalReferenceables.Count > 0)
			{
				allReferenceables = new List<ISerializableRoot>(allReferenceables);
				allReferenceables.AddRange(externalReferenceables);
			}

			List<ISerializableRoot> serializables;
			List<SerializerError> errors;
			serializer.Deserialize(new string[] { data }, new string[] { filename }, allReferenceables, out serializables, out errors);

			this.errors.AddRange(errors);
			foreach (var s in serializables)
			{
				allRoots.Add(s);
				if (s is IPrototype)
					prototypes.Add(s as IPrototype);
			}
		}

		/// <summary>
		/// Same as <see cref="Parse(string, string)"/>, but can parse multiple data strings together.
		/// 
		/// The prototypes will be loaded in order and able to resolve references across multiple files!
		/// </summary>
		public void Parse(string[] data, string[] filenames, List<ISerializableRoot> externalReferenceables = null)
		{
			List<ISerializableRoot> allReferenceables = allRoots;
			if (externalReferenceables != null && externalReferenceables.Count > 0)
			{
				allReferenceables = new List<ISerializableRoot>(allReferenceables);
				allReferenceables.AddRange(externalReferenceables);
			}

			List<ISerializableRoot> serializables;
			List<SerializerError> errors;
			serializer.Deserialize(data, filenames, allReferenceables, out serializables, out errors);
			
			this.errors.AddRange(errors);
			foreach (var s in serializables)
			{
				allRoots.Add(s);
				if (s is IPrototype)
					prototypes.Add(s as IPrototype);
			}
		}
	}
}