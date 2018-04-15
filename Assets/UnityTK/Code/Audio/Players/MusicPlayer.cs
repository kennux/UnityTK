using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Music player used to play back a collection of <see cref="MusicTrack"/>.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        [Header("References")]
        /// <summary>
        /// The audio source for playback.
        /// </summary>
        public AudioSource audioSource;

        /// <summary>
        /// The tracks to be played back.
        /// </summary>
        public List<MusicTrack> tracks = new List<MusicTrack>();

        /// <summary>
        /// Whether or not the player will play the tracks in order as defined.
        /// </summary>
        [Header("Configuration")]
        public bool randomOrder;

        /// <summary>
        /// The <see cref="tracks"/> item that was played last time some playback started (i.e. the track that is currently playing).
        /// This is ignored when the player is running with <see cref="randomOrder"/> true.
        /// </summary>
        private int currentlyPlayed = -1;

        /// <summary>
        /// The track being currently played.
        /// </summary>
        public MusicTrack currentTrack
        {
            get
            {
                if (this.currentlyPlayed < 0 || this.currentlyPlayed >= this.tracks.Count)
                    return null;
                return this.tracks[this.currentlyPlayed];
            }
        }

        /// <summary>
        /// Called in order to play the next track.
        /// </summary>
        private void PlayNextTrack()
        {
            // Determine what to play next
            int nextIndex = this.currentlyPlayed;
            if (this.randomOrder)
            {
                nextIndex = Random.Range(0, this.tracks.Count);
            }
            else
            {
                nextIndex++;
                if (nextIndex >= this.tracks.Count)
                    nextIndex = 0;
            }

            this.audioSource.Stop();
            this.tracks[nextIndex].Play(this.audioSource);
        }

        public void Update()
        {
            if (!this.audioSource.isPlaying)
                PlayNextTrack();
        }
    }
}