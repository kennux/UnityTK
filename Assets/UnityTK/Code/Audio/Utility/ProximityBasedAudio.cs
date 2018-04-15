using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Audio source controller implementation.
    /// Calculates proximity based sound volume and stereo panning.
    /// 
    /// It will map the gameobject's position to the screen space using <see cref="ProximityPlayer"/>.
    /// The resulting screen space position then is used to calculate volume and stereo panning.
    /// 
    /// The underlying audio source will be using 2d spatial blending and stereo panning, not 3d effects.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ProximityBasedAudio : MonoBehaviour
    {
        public AudioSource audioSource
        {
            get { return this._audioSource.Get(this); }
        }
        private LazyLoadedComponentRef<AudioSource> _audioSource = new LazyLoadedComponentRef<AudioSource>();

        [Header("Configuration")]
        [Range(0, 1)]
        public float volume;

        /// <summary>
        /// The animation curve used to calculate the audio fadeout.
        /// The time axis is mapped to the distance of this audio source in the camera screenspace.
        /// </summary>
        public AnimationCurve proximityFadeout;
        public bool getSettingsFromAudioSourceOnEnable = true;

        private void OnValidate()
        {
            if (ReferenceEquals(this.proximityFadeout, null))
            {
                this.proximityFadeout = AnimationCurve.Linear(0, 1, 2, 0);
                this.audioSource.spatialBlend = 0;
            }
        }

        public void OnEnable()
        {
            if (this.getSettingsFromAudioSourceOnEnable)
                this.volume = this.audioSource.volume;
        }

        public void Update()
        {
            float panStereo;
            float proximity = ProximityPlayer.instance.GetProximity(this.transform, out panStereo);

            float volume = this.proximityFadeout.Evaluate(proximity) * this.volume;
            this.audioSource.volume = volume;
            this.audioSource.panStereo = panStereo;
        }
    }
}