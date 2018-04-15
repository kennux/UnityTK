using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    [CreateAssetMenu(fileName = "MusicTrack", menuName = "UnityTK/Audio/Music Track")]
    public class MusicTrack : AudioEvent
    {
        public string title;
        public string interpret;

        /// <summary>
        /// The clip to play.
        /// </summary>
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