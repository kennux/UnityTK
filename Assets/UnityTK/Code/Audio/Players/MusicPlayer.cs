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
        /// <summary>
        /// Delegate for <see cref="onTrackPlay"/>
        /// </summary>
        public delegate void OnTrackPlay(MusicTrack track);

        [Header("References")]
        /// <summary>
        /// The audio source for playback.
        /// </summary>
        public AudioSource audioSource;

        /// <summary>
        /// The audio source for playback of the next track (for fading).
        /// </summary>
        public AudioSource audioSourceNext;

        /// <summary>
        /// The tracks to be played back.
        /// </summary>
        public List<MusicTrack> tracks = new List<MusicTrack>();

        /// <summary>
        /// Invoked whenever a new track is being played.
        /// It is called before the track playing started with the track that is about to be played.
        /// </summary>
        public event OnTrackPlay onTrackPlay;

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
        /// The <see cref="tracks"/> item that will be played when the next track is being played.
        /// </summary>
        private int nextPlayed = -1;

        /// <summary>
        /// Whether or not to start playing the music in <see cref="Awake"/>
        /// </summary>
        public bool playOnAwake = true;

        /// <summary>
        /// Determines whether or not this player is currently playing music.
        /// </summary>
        public bool isPlaying
        {
            get { return this._isPlaying; }
        }

        private bool _isPlaying;

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
        /// The track being played next.
        /// </summary>
        public MusicTrack nextTrack
        {
            get
            {
                if (this.nextPlayed < 0 || this.nextPlayed >= this.tracks.Count)
                    return null;
                return this.tracks[this.nextPlayed];
            }
        }

        private void OnValidate()
        {
            if (Essentials.UnityIsNull(this.audioSource))
                this.audioSource = this.GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (this.playOnAwake)
                this.Play();
        }

        /// <summary>
        /// Starts playing music on this player.
        /// </summary>
        public void Play()
        {
            this._isPlaying = true;

            this.nextPlayed = GetNextTrackIndex();
            this.PlayNextTrack();
        }

        /// <summary>
        /// Stops playing music on this player.
        /// </summary>
        public void Stop()
        {
            if (this._isPlaying)
            {
                this.audioSource.Stop();
                this._isPlaying = false;
            }
        }

        private int GetNextTrackIndex()
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

            return nextIndex;
        }

        /// <summary>
        /// Called in order to play the next track.
        /// </summary>
        private void PlayNextTrack()
        {
            this.currentlyPlayed = this.nextPlayed;
            this.nextPlayed = GetNextTrackIndex();

            // Select track
            var track = this.tracks[this.currentlyPlayed];

            // Fire event
            if (!ReferenceEquals(this.onTrackPlay, null))
                this.onTrackPlay(track);

            // Stop and play
            this.audioSource.Stop();
            track.Play(this.audioSource);
        }

        public void Update()
        {
            if (!this._isPlaying)
                return;

            if (!this.audioSource.isPlaying)
                PlayNextTrack();
            else
            {
                // TODO: Fading
            }
        }
    }
}