using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Audio event for non-spatial playback.
    /// Will play the sound without spatial blending at centric stereo panning.
    /// </summary>
    [CreateAssetMenu(fileName = "NonSpatialAudioEvent", menuName = "UnityTK/Audio/Non Spartial Audio Event")]
    public class NonSpatialAudioEvent : AudioEvent
    {
        /// <summary>
        /// The volume range in which to play this event.
        /// </summary>
        [MinMaxRange(0, 1)]
        public RangedFloat volume = new RangedFloat(1, 1);

        /// <summary>
        /// The pitch range in which to play this event.
        /// </summary>
        [MinMaxRange(0, 2)]
        public RangedFloat pitch = new RangedFloat(1, 1);

        /// <summary>
        /// The clip to play.
        /// </summary>
        public AudioClip clip;

        /// <summary>
        /// Plays <see cref="clip"/>
        /// </summary>
        public override void Play(AudioSource audioSource)
        {
            audioSource.spatialBlend = 0;
            audioSource.panStereo = 0;
            audioSource.volume = this.volume.GetRandomInRange();
            audioSource.pitch = this.pitch.GetRandomInRange();
            audioSource.clip = this.clip;
            audioSource.Play();
        }
    }
}