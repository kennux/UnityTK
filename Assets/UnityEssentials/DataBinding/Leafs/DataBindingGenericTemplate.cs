using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEssentials.DataBinding
{
    /// <summary>
    /// Generic data binding leaf (<see cref="DataBindingGenericTemplatedLeaf"/>) template asset implementation.
    /// Bind templates can be used to define the target the leaf will be binding to.
    /// 
    /// For example a generic bind template that defines it binds to a unity ui label's text field.
    /// The generic field then only needs this template and its target binding field assigned.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "UnityEssentials/DataBindings/Generic Leaf Template", fileName = "GenericLeafTemplate")]
    public class DataBindingGenericTemplate : ScriptableObject
    {
        /// <summary>
        /// The target type, the string passed in to <see cref="Type.GetType(string)"/>
        /// </summary>
        [SerializeField]
        private string targetType;

        /// <summary>
        /// The field / property name on <see cref="targetType"/> that this template will bind to.
        /// </summary>
        [SerializeField]
        private string targetField;

        /// <summary>
        /// Cached type reference.
        /// </summary>
        private Type _type;

        /// <summary>
        /// Cache type field property.
        /// </summary>
        private DataBindingFieldProperty _field;

        /// <summary>
        /// Set in <see cref="OnValidate"/> to reflect input data validity.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        private bool _isValid;

        [ContextMenu("Validate")]
        public void OnValidate()
        {
            this._type = null;
            this._field = null;

            this._isValid = !object.ReferenceEquals(this.GetTargetType(), null) && !object.ReferenceEquals(this.GetField(), null);
        }

        /// <summary>
        /// Returns the target type this template will bind to.
        /// </summary>
        public Type GetTargetType()
        {
            if (object.ReferenceEquals(this._type, null))
            {
                this._type = Type.GetType(this.targetType);

                if (object.ReferenceEquals(this._type, null))
                    Debug.LogError("Couldnt find type " + this.targetType);
            }

            return this._type;
        }

        /// <summary>
        /// Returns the field this template binds to.
        /// </summary>
        public DataBindingFieldProperty GetField()
        {
            if (object.ReferenceEquals(this._field, null))
            {
                var type = GetTargetType();
                this._field = DataBindingFieldProperty.Get(type, this.targetField);

                if (object.ReferenceEquals(this._field, null))
                    Debug.LogError("Couldnt find field " + this.targetField + " on type " + this.targetType + " ( " + type + " )");
            }

            return this._field;
        }
        
        /// <summary>
        /// Returns the type of the field this template binds to.
        /// </summary>
        public System.Type GetFieldType()
        {
            return GetField().fieldType;
        }
    }
}
