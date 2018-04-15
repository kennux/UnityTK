using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    [CreateAssetMenu(fileName = "MusicTrack", menuName = "UnityTK/Audio/Music Track")]
    public class MusicTrack : AudioEvent
    {
        [Header("Metadata")]
        public string title;
        public string interpret;

        [Header("Fading")]
        public float fadeoutTime;
        public float fadeInTime;
        public AnimationCurve fadeCurve;

        /// <summary>
        /// The clip to play.
        /// </summary>
        [Header("Configuration")]
        public AudioClip clip;

        /// <summary>
        /// Plays <see cref="clip"/>
        /// </summary>
        public override void Play(AudioSource audioSource)
        {
            audioSource.clip = this.clip;
            audioSource.Play();
        }
    }
}