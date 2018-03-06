using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials.DataBinding
{
    /// <summary>
    /// Abstract implementation of an arbitrary databinding.
    /// The binding can be a node (root or branch) or a leaf.
    /// </summary>
    public abstract class DataBinding : MonoBehaviour
    {
        /// <summary>
        /// The object this databinding is binding to.
        /// </summary>
        public object boundObject
        {
            get
            {
                return this.GetBoundObject();
            }
        }

        /// <summary>
        /// The type this binding was bound to (the type of <see cref="boundObject"/>).
        /// </summary>
        public System.Type boundType
        {
            get
            {
                return this.GetBoundType();
            }
        }

        /// <summary>
        /// The parent binding node.
        /// Roots might not have a parent (null).
        /// </summary>
        public abstract DataBinding parent
        {
            get;
        }

        /// <summary>
        /// Registers this binding to its parent, if there is a parent.
        /// </summary>
        public virtual void Awake()
        {
            var parent = this.parent;
            if (object.ReferenceEquals(parent, null))
                return;

            parent.RegisterChild(this);
        }

        /// <summary>
        /// Returns the type this databinding was bound to.
        /// </summary>
        protected abstract System.Type GetBoundType();

        /// <summary>
        /// Retrives the object this binding is binding to.
        /// </summary>
        protected abstract object GetBoundObject();

        /// <summary>
        /// Called whenever this binding needs to be updated.
        /// </summary>
        public abstract void UpdateBinding();

        /// <summary>
        /// Returns the target type this databinding is accepting.
        /// This is not necessarily the actual type of the <see cref="boundObject"/>.
        /// 
        /// It is used to determine which types can be assigned to this databinding.
        /// In most cases this will be the upper most type in the inheritance tree this binding can accept.
        /// For example a root that can bind to any object will return object as target type, while a text leaf will return string for example.
        /// </summary>
        public abstract System.Type GetBindTargetType();

        /// <summary>
        /// Can be overridden to implement child registration.
        /// Default implementation throws <see cref="System.NotImplementedException"/>
        /// </summary>
        protected virtual void RegisterChild(DataBinding child)
        {
            throw new System.NotImplementedException();
        }
    }
}