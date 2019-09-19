using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTK.Audio
{
	/// <summary>
	/// "Normal" spatial audio source implementation, essentially a wrapper around <see cref="AudioSource"/> from the unity engine.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class UTKAudioSource : MonoBehaviour, IUTKAudioSource
	{
		public virtual AudioSource underlying
		{
			get { return this._underlying.Get(this); }
		}
		private LazyLoadedComponentRef<AudioSource> _underlying = new LazyLoadedComponentRef<AudioSource>();

		public virtual float volume
		{
			get { return this._volume; }
			set { this._volume = value; UpdateVolume(); }
		}
		[Range(0,1)]
		[SerializeField]
		private float _volume = 1;

		/// <summary>
		/// The volume of the currently played clip.
		/// This will be multiplied with <see cref="volume"/>
		/// </summary>
		public virtual float clipVolume
		{
			get { return this._clipVolume; }
			set { this._clipVolume = value; UpdateVolume(); }
		}
		private float _clipVolume = 1;
		
		public virtual float pitch
		{
			get { return this._pitch; }
			set { this._pitch = value; UpdatePitch(); }
		}
		[Range(-3,3)]
		[SerializeField]
		private float _pitch = 1;

		/// <summary>
		/// The pitch of the currently played clip.
		/// This will be multiplied with <see cref="pitch"/>
		/// </summary>
		public virtual float clipPitch
		{
			get { return _clipPitch; }
			set { _clipPitch = value; UpdatePitch(); }
		}
		private float _clipPitch = 1;

		public virtual AudioClip clip
		{
			get { return this.underlying.clip; }
			set { this.underlying.clip = value; }
		}

		public virtual bool isPlaying
		{
			get { return this.underlying.isPlaying; }
		}

		public virtual float minDistance
		{
			get { return this.underlying.minDistance; }
			set { this.underlying.minDistance = value; }
		}

		public virtual float maxDistance
		{
			get { return this.underlying.maxDistance; }
			set { this.underlying.maxDistance = value; }
		}

		public virtual float time
		{
			get { return this.underlying.time; }
			set { this.underlying.time = value; }
		}

		public virtual AudioRolloffMode rolloffMode
		{
			get { return this.underlying.rolloffMode; }
			set { this.underlying.rolloffMode = value; }
		}

		protected virtual void UpdateVolume()
		{
			this.underlying.volume = this._volume * this._clipVolume;
		}

		protected virtual void UpdatePitch()
		{
			this.underlying.pitch = this._pitch * this._clipPitch;
		}

		public virtual void Play(AudioClip clip, bool loop = false)
		{
			this.clip = clip;
			this.underlying.loop = loop;
			this.underlying.Play();
		}

		public virtual void Stop()
		{
			this.clip = null;
			this.underlying.Stop();
		}

		public virtual void ResetConfig()
		{
			this.volume = 1;
			this.pitch = 1;
			this.clipVolume = 1;
			this.clipPitch = 1;
			this.clip = null;
			this.minDistance = 1;
			this.maxDistance = 500;
			this.rolloffMode = AudioRolloffMode.Logarithmic;
		}
	}
}
