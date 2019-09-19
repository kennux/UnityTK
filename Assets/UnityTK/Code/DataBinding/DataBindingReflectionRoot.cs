using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.DataBinding
{
	public enum DataBindingUpdateMode
	{
		Update,
		LateUpdate,
		FixedUpdate
	}

	/// <summary>
	/// Abstract implementation for databinding roots which work by reflection.
	/// Used in <see cref="DataBindingRoot"/> and <see cref="DataBindingScriptedRoot"/>
	/// </summary>
	public abstract class DataBindingReflectionRoot : DataBindingReflectionNode
	{
		/// <summary>
		/// The unity message used to run databinding updates.
		/// The standard value is LateUpdate, since most of the time gameobjects are being enabled / disabled by other gameobjects in Update(),
		/// In order to still get updated after being enabled, LateUpdate is used as it is fired right after Update and unity won't fire Update on objects enabled from an Update call.
		/// </summary>
		public DataBindingUpdateMode updateMode = DataBindingUpdateMode.LateUpdate;

		/// <summary>
		/// The update framerate.
		/// </summary>
		public int updateFramerate = 20;

		/// <summary>
		/// Set to true in <see cref="OnEnable"/>, will force a binding update in the next <see cref="Update"/>.
		/// </summary>
		private bool justEnabled = false;

		/// <summary>
		/// The update time calculated from <see cref="updateFramerate"/>
		/// </summary>
		private float updateTime { get { return 1f / (float)this.updateFramerate; } }

		/// <summary>
		/// Time passed since last update
		/// </summary>
		private float _time;

		/// <summary>
		/// Returns null always, since roots dont have a parent.
		/// </summary>
		public override DataBinding parent
		{
			get { return null; }
		}

		protected override void DoUpdateBinding()
		{

		}

		public void OnEnable()
		{
			this.justEnabled = true;
		}

		protected virtual void FrameUpdate()
		{
			this._time += Time.deltaTime;

			if (this.justEnabled || this._time > this.updateTime)
			{
				this.UpdateBinding();
				this._time = 0;
				this.justEnabled = false;
			}
		}

		public void Update()
		{
			if (updateMode == DataBindingUpdateMode.Update)
				FrameUpdate();
		}

		public void FixedUpdate()
		{
			if (updateMode == DataBindingUpdateMode.FixedUpdate)
				FrameUpdate();
		}

		public void LateUpdate()
		{
			if (updateMode == DataBindingUpdateMode.LateUpdate)
				FrameUpdate();
		}
	}
}