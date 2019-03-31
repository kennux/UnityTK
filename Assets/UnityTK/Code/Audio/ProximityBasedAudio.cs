using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// UnityTK audio source implementation for proximity based audio playback and object tracking.
    /// Calculates proximity based sound volume and stereo panning.
    /// 
    /// It will map the gameobject's position to the screen space using <see cref="ProximityPlayer"/>.
    /// The resulting screen space position then is used to calculate volume and stereo panning.
    /// 
    /// The underlying audio source will be using 2d spatial blending and stereo panning, not 3d effects.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ProximityBasedAudio : UTKAudioSource
    {
        // Disable setting properties of spatial audio
        public override float minDistance
        {
            get
            {
                return base.minDistance;
            }

            set
            {
            }
        }

        public override float maxDistance
        {
            get
            {
                return base.maxDistance;
            }

            set
            {
            }
        }

        public override AudioRolloffMode rolloffMode
        {
            get
            {
                return base.rolloffMode;
            }

            set
            {
            }
        }

        /// <summary>
        /// The animation curve used to calculate the audio fadeout.
        /// The time axis is mapped to the distance of this audio source in the camera screenspace.
        /// </summary>
        public AnimationCurve proximityFadeout;

        private void OnValidate()
        {
            if (ReferenceEquals(this.proximityFadeout, null))
            {
                this.proximityFadeout = AnimationCurve.Linear(0, 1, 2, 0);
                this.underlying.spatialBlend = 0;
            }
        }

        public override void Play(AudioClip clip, bool loop = false)
        {
            base.Play(clip, loop);
            this.Update();
        }

        protected override void UpdateVolume()
        {
            base.UpdateVolume();
            
            float panStereo;
            float proximity = ProximityPlayer.instance.GetProximity(this.transform, out panStereo);

            float volume = this.proximityFadeout.Evaluate(proximity) * underlying.volume;
            this.underlying.volume = volume;
            this.underlying.panStereo = panStereo;
        }

        public void Update()
        {
            UpdateVolume();
        }

        public override void ResetConfig()
        {
            base.ResetConfig();

            this.underlying.spatialBlend = 0;
            this.underlying.panStereo = 0;
        }
    }
}