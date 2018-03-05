using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials.DataBinding
{
    /// <summary>
    /// Implements abstract data binding leaf behaviour.
    /// 
    /// Leaves can bind to a field on a parent <see cref="DataBindingNode"/> in order to replicate the bound value onto the leaf's target.
    /// </summary>
    public abstract class DataBindingLeaf : DataBinding
    {
        /// <summary>
        /// Invoked when the target this leaf binds to was changed.
        /// This will update the value this leaf binds to and can be used to achieve 2-way bindings.
        /// </summary>
        public abstract void OnChanged();
    }
}