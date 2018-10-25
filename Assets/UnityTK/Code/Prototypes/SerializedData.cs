using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Prototypes
{
	internal class SerializedData
	{
		public string name;
		public string inherits;
		
		private Dictionary<string, object> finalizedFields = new Dictionary<string, object>();
		private Dictionary<string, object> fields = new Dictionary<string, object>();
		private Dictionary<SerializedData, object> subInstances = new Dictionary<SerializedData, object>();

		class PrototypeReference
		{
			public string name;

			public IPrototype Resolve(List<IPrototype> prototypes)
			{
				foreach (var p in prototypes)
				{
					if (string.Equals(p.name, this.name))
						return p;
				}

				return null;
			}
		}

		private SerializableTypeCache targetType;

		private XElement element;
		public string filename { get; private set; }

		public void ClearFields()
		{
			this.fields.Clear();
		}

		public void PrepareParse(SerializableTypeCache targetType, XElement element, string filename)
		{
			this.targetType = targetType;
			this.name = element.Attribute(Prototypes.PrototypeAttributeName).Value;
			this.element = element;
			this.filename = filename;

			var inheritsAttrib = element.Attribute(Prototypes.PrototypeAttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
			}
		}

		public void ParseFields(List<ParsingError> errors)
		{
			foreach (var node in element.Nodes())
			{
				if (!(node is XElement)) // Malformed XML
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (node as IXmlLineInfo).LineNumber, "Unable to cast node to element for " + node + "! Skipping element!"));
					continue;
				}
				var nodeElement = node as XElement;

				// Parse element recursively
				if (nodeElement.HasElements)
					fields.Set(nodeElement.Name.LocalName, nodeElement);
				else
					fields.Set(nodeElement.Name.LocalName, nodeElement.Value);
			}
		}

		public void PreLoadFields(List<ParsingError> errors)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();
			List<string> removeFields = ListPool<string>.Get();

			try
			{
				// First, resolve elements left in the fields dictionary
				foreach (var field in fields)
				{
					// Field unknown?
					if (!targetType.HasField(field.Key))
					{
						// TODO: Line number
						errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Unknown field " + field.Key + "! Skipping field!"));
						removeFields.Add(field.Key);
						continue;
					}

					var fieldData = targetType.GetFieldData(field.Key);
					var element = field.Value;
					if (element is XElement)
					{
						// Field not serializable?
						if (ReferenceEquals(fieldData.serializableTypeCache, null))
						{
							// TODO: Line number
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Field of type " + fieldData.fieldInfo.FieldType + " is unknown by the serializer cache! Are you missing " + nameof(PrototypesTypeSerializerAttribute) +" attribute? Skipping field!"));
							removeFields.Add(field.Key);
							continue;
						}

						// Resolve field name type
						var d = new SerializedData();
						d.PrepareParse(fieldData.serializableTypeCache, element as XElement, this.filename);
						d.ParseFields(errors);
						d.PreLoadFields(errors);
						subInstances.Add(d, d.Create());
						updates.Add(field.Key, d);
					}
					else if (element is string)
					{
						if (fieldData.isPrototype)
						{
							// This is a reference!
							updates.Add(field.Key, new PrototypeReference()
							{
								name = element as string
							});
						}
						else
						{
							try
							{
								var serializer = PrototypesCaches.GetSerializerFor(fieldData.fieldInfo.FieldType);
								if (ReferenceEquals(serializer, null))
								{
									errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer for field " + field.Key + " on type " + targetType.type + " (" + fieldData.fieldInfo.FieldType + ") could not be found! Skipping field!"));
									removeFields.Add(field.Key);
									continue;
								}

								updates.Add(field.Key, serializer.Deserialize(field.Value as string));
							}
							catch (Exception ex)
							{
								errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + field.Key + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
								removeFields.Add(field.Key);
							}
						}
					}
				}

				// Write updates
				foreach (var update in updates)
					this.fields[update.Key] = update.Value;

				// Remove fields
				foreach (var field in removeFields)
					this.fields.Remove(field);
			}
			finally
			{
				DictionaryPool<string, object>.Return(updates);
				ListPool<string>.Return(removeFields);
			}
		}

		public object Create()
		{
			object obj = Activator.CreateInstance(this.targetType.type);
			return obj;
		}

		public void Apply(object obj, List<ParsingError> errors)
		{
			foreach (var field in fields)
			{
				var fieldInfo = this.targetType.GetFieldData(field.Key);
				if (!ReferenceEquals(field.Value, null) && !fieldInfo.fieldInfo.FieldType.IsAssignableFrom(field.Value.GetType()))
				{
					errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Fatal error deserializing field " + field.Key + " - tried applying field data but types mismatched! Stored type: " + field.Value.GetType() + " - Declared type: " + fieldInfo.fieldInfo.FieldType + "! Skipping field!"));
					continue;
				}

				fieldInfo.fieldInfo.SetValue(obj, field.Value);
			}
		}

		public void ResolveReferenceFieldsAndSubData(object obj, List<IPrototype> prototypes, List<ParsingError> errors)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();

			try
			{
				foreach (var field in fields)
				{
					var @ref = field.Value as PrototypeReference;
					if (!ReferenceEquals(@ref, null))
					{
						updates.Add(field.Key, @ref.Resolve(prototypes));
					}

					var sub = field.Value as SerializedData;
					if (!ReferenceEquals(sub, null))
					{
						sub.ResolveReferenceFieldsAndSubData(subInstances[sub], prototypes, errors);
						updates.Add(field.Key, subInstances[sub]);
					}
				}
				
				// Write updates
				foreach (var update in updates)
					this.fields[update.Key] = update.Value;
			}
			finally
			{
				DictionaryPool<string, object>.Return(updates);
			}
		}

		public IEnumerable<string> GetReferencedPrototypes()
		{
			foreach (var field in fields)
			{
				var v = field.Value as PrototypeReference;
				if (!ReferenceEquals(v, null))
					yield return v.name;

				var sub = field.Value as SerializedData;
				if (!ReferenceEquals(sub, null))
					foreach (var @ref in sub.GetReferencedPrototypes())
						yield return @ref;
			}
		}

		public void FinalizeLoadedFields()
		{
			foreach (var f in fields)
			{
				this.finalizedFields.Add(f.Key, f.Value);
			}
			this.fields.Clear();
		}

		public SerializedData DeepCopy()
		{
			return null; // TODO
		}
	}
}
