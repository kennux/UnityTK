using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Manager behaviour / singleton player implementation that can be used to oneshot play <see cref="AudioEvent"/>.
    /// </summary>
    public class AudioOneShotPlayer : MonoBehaviour
    {
        /// <summary>
        /// Playback type
        /// </summary>
        public enum PlaybackType
        {
            WORLDSPACE,
            PROXIMITY
        }

        /// <summary>
        /// Playback datastructure.
        /// </summary>
        public struct Playback
        {
            public PlaybackType type;
            public AudioSource source;
            public AudioEvent evt;
        }

        public static AudioOneShotPlayer instance { get { return UnitySingleton<AudioOneShotPlayer>.Get(); } }

        [Header("Prefabs")]
        public AudioSource worldspacePrefab;
        public ProximityBasedAudio proximityBasedPrefab;

        /// <summary>
        /// Audio sources used for worldspace playback.
        /// </summary>
        private ObjectPool<AudioSource> worldspaceAudioSources;

        /// <summary>
        /// Audio sources used for proximity based playback.
        /// </summary>
        private ObjectPool<AudioSource> proximityAudioSources;

        /// <summary>
        /// All currently ongoing playbacks.
        /// </summary>
        private List<Playback> playbacks = new List<Playback>();

        public void Awake()
        {
            UnitySingleton<AudioOneShotPlayer>.Register(this);
            this.worldspaceAudioSources = new ObjectPool<AudioSource>(this.CreateWorldspaceAudioSource, 250, this.ReturnWorldspaceAudioSource);
            this.proximityAudioSources = new ObjectPool<AudioSource>(this.CreateProximityAudioSource, 250, this.ReturnProximityAudioSource);
        }

        private void ReturnProximityAudioSource(AudioSource source)
        {
            source.Stop();

            source.gameObject.SetActive(false);
            source.transform.parent = this.transform;
        }

        private void ReturnWorldspaceAudioSource(AudioSource source)
        {
            source.Stop();

            source.spatialBlend = 1;
            source.panStereo = 0;

            source.gameObject.SetActive(false);
            source.transform.parent = this.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
        }

        private AudioSource CreateProximityAudioSource()
        {
            // Create
            GameObject go = Instantiate(this.proximityBasedPrefab.gameObject);
            AudioSource source = go.GetComponent<AudioSource>();
            ProximityBasedAudio proximity = go.GetComponent<ProximityBasedAudio>();

            // TODO: Proximity stuff

            // Prepare
            ReturnWorldspaceAudioSource(source);

            return source;
        }

        private AudioSource CreateWorldspaceAudioSource()
        {
            // Create
            GameObject go = Instantiate(this.worldspacePrefab.gameObject);
            AudioSource source = go.GetComponent<AudioSource>();

            // Prepare
            ReturnWorldspaceAudioSource(source);

            return source;
        }

        public void Update()
        {
            // Figure out which audio sources stopped playing audio
            List<int> stopped = ListPool<int>.Get();
            for (int i = 0; i < this.playbacks.Count; i++)
            {
                if (!this.playbacks[i].source.isPlaying)
                    stopped.Add(i);
            }

            // Remove all stopped audio sources
            for (int i = 0; i < stopped.Count; i++)
            {
                var playback = this.playbacks[stopped[i]];
                this.playbacks.RemoveAt(stopped[i] - i);

                ObjectPool<AudioSource> pool = null;
                switch (playback.type)
                {
                    case PlaybackType.PROXIMITY: pool = this.proximityAudioSources; break;
                    case PlaybackType.WORLDSPACE: pool = this.worldspaceAudioSources; break;
                }

                pool.Return(playback.source);
            }

            ListPool<int>.Return(stopped);
        }

        /// <summary>
        /// Plays the specified event once on the specified player gameobject.
        /// This will make the playback audio source be parented to player at origin position and identity rotation.
        /// 
        /// Note that this method is not doing any spatial playback configuration on the audio source _at all_.
        /// The audio event should do the spatial setup.
        /// 
        /// Playback is done with an audio source which has the <see cref="ProximityBasedAudio"/> component.
        /// </summary>
        /// <param name="evt">The event to play</param>
        /// <param name="player">The object which is playing the event.</param>
        public Playback PlayProximity(AudioEvent evt, GameObject player)
        {
            var source = this.worldspaceAudioSources.Get();

            source.transform.parent = this.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            source.gameObject.SetActive(true);
            evt.Play(source);

            var playback = new Playback()
            {
                evt = evt,
                source = source,
                type = PlaybackType.PROXIMITY
            };
            this.playbacks.Add(playback);
            return playback;
        }

        /// <summary>
        /// Plays the specified event once on the specified player gameobject.
        /// This will make the playback audio source be parented to player at origin position and identity rotation.
        /// 
        /// Note that this method is not doing any spatial playback configuration on the audio source _at all_.
        /// The audio event should do the spatial setup.
        /// </summary>
        /// <param name="evt">The event to play</param>
        /// <param name="player">The object which is playing the event.</param>
        /// <returns>Playback information</returns>
        public Playback PlayWorldspace(AudioEvent evt, GameObject player)
        {
            var source = this.worldspaceAudioSources.Get();

            source.transform.parent = this.transform;
            source.transform.localPosition = Vector3.zero;
            source.transform.localRotation = Quaternion.identity;
            source.gameObject.SetActive(true);
            evt.Play(source);

            var playback = new Playback()
            {
                evt = evt,
                source = source,
                type = PlaybackType.WORLDSPACE
            };
            this.playbacks.Add(playback);
            return playback;
        }
    }
}