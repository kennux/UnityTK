using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTK.Audio
{
    /// <summary>
    /// Plays an <see cref="AudioEvent"/>
    /// </summary>
    public class AudioOneShotOnClick : MonoBehaviour, IPointerClickHandler
    {
        public AudioEvent audioEvent;

        /// <summary>
        /// Optional parameter, the audio source used for playing the event.
        /// If not specified, <see cref="AudioOneShotPlayer"/> api will be used!
        /// </summary>
        [Tooltip("Optional parameter, the audio source used for playing the event. If not specified, AudioOneShotPlayer api will be used!")]
        public AudioSource audioSource;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Essentials.UnityIsNull(this.audioSource))
                AudioOneShotPlayer.instance.PlayWorldspace(this.audioEvent, this.gameObject);
            else
                this.audioEvent.Play(this.audioSource);
        }
    }
}