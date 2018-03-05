using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEssentials.DataBinding
{
    /// <summary>
    /// Base class for <see cref="DataBindingNode"/>s which bind to an arbitrary object (<see cref="DataBinding.boundObject"/>) and provide its fields and properties via reflections as binding fields.
    /// </summary>
    public abstract class DataBindingReflectionNode : DataBindingNode
    {
        /// <summary>
        /// Cache mapping <see cref="GetFieldValue(string)"/> to their field property.
        /// </summary>
        protected Cache<string, DataBindingFieldProperty> fieldCache
        {
            get
            {
                if (object.ReferenceEquals(this._fieldCache, null))
                    this._fieldCache = new Cache<string, DataBindingFieldProperty>(CacheConstructor);

                return this._fieldCache;
            }
        }
        private Cache<string, DataBindingFieldProperty> _fieldCache;

        public override Type GetBindTargetType()
        {
            return typeof(object);
        }

        /// <summary>
        /// <see cref="DataBinding.boundObject"/> object equality to null check.
        /// </summary>
        public bool hasBoundObject { get { return !object.Equals(this.boundObject, null); } }

        /// <summary>
        /// Cache element constructor for <see cref="fieldCache"/>
        /// </summary>
        private DataBindingFieldProperty CacheConstructor(string field)
        {
            return DataBindingFieldProperty.Get(this.boundObject.GetType(), field);
        }

        public override void SetFieldValue(string field, object value)
        {
            object boundObject = this.boundObject; // Invoke getter once
            if (object.ReferenceEquals(boundObject, null))
                throw new NullReferenceException("Bound object is null, cannot set a field of null!");

            // Get field prop & set
            DataBindingFieldProperty fieldProperty = DataBindingFieldProperty.Get(boundObject.GetType(), field);
            fieldProperty.SetValue(boundObject, value);
        }

        public override List<string> GetFields(System.Type type, List<string> preAlloc = null)
        {
            ListPool<string>.GetIfNull(ref preAlloc);

            // No target set? If so, return empty list
            if (!this.hasBoundObject)
                return preAlloc;

            // Write fields
            var props = DataBindingFieldProperty.Get(this.boundObject.GetType());
            for (int i = 0; i < props.Count; i++)
            {
                if (type.IsAssignableFrom(props[i].fieldType))
                    preAlloc.Add(props[i].name);
            }

            return preAlloc;
        }

        public override object GetFieldValue(string field)
        {
            if (!this.hasBoundObject || string.IsNullOrEmpty(field))
                return null;

            return this.fieldCache.Get(field).GetValue(this.boundObject);
        }
    }
}
