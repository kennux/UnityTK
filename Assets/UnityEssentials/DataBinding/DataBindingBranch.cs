using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials.DataBinding
{
    /// <summary>
    /// Databinding tree branch node implementation.
    /// The branch can bind to any arbitrary parent node and provide one of its field for binding by childs.
    /// </summary>
    public class DataBindingBranch : DataBindingReflectionNode
    {
        /// <summary>
        /// The parent node this branch is binding to.
        /// </summary>
        [ParentComponent(typeof(DataBindingNode))]
        public DataBindingNode parentNode;

        /// <summary>
        /// The field on the <see cref="parentNode"/> this branch is binding to.
        /// </summary>
        [DataBindingField]
        public string field;

        public override DataBinding parent
        {
            get { return this.parentNode; }
        }

        protected override void DoUpdateBinding()
        {

        }

        protected override object GetBoundObject()
        {
            return this.parentNode.GetFieldValue(this.field);
        }
    }
}