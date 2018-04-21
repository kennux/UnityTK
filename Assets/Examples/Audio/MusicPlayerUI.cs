using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.Audio;

/// <summary>
/// Quick and dirty model for databindings to listen to <see cref="UnityTK.Audio.MusicPlayer.onTrackPlay"/>
/// </summary>
public class MusicPlayerUI : MonoBehaviour
{
    [Header("Config")]
    public MusicPlayer player;

    [Header("DB Props")]
    public bool visible;
    public string interpret;
    public string title;

    public void Awake()
    {
        this.player.onTrackPlay += OnTrackPlay;
    }

    private void OnTrackPlay(MusicTrack track)
    {
        Invoke("Hide", 5f);
        this.visible = true;
        this.interpret = track.interpret;
        this.title = track.title;
    }

    private void Hide()
    {
        this.visible = false;
    }
}
