# Audio

The UnityTK audio system provides a very simple and lightweight abstraction layer on top of the unity engine audio system.
It provides the ability to construct game sound systems using events.

## AudioEvents

Audio events describe a specific sound to be played on a specific event happening (like a gun being shot).
This abstract layer is used to very easily tweak sound behaviour with scriptable objects.

### SimpleAudioEvent

Audio event implementation which simply plays an AudioClip on an arbitrary UnityTK audio source implementation.
It is designed for spatial audio playback, but can also be used for non-spatial playback.

### MusicTrack

Specialized audio event to be used for music playback with the MusicPlayer.

## UnityTK AudioSource

The audio system is built upon an audio source abstraction layer on top of the unity audio system.
This layer gives UnityTK the ability to implement arbitrary audio playback behaviour.

### UTKAudioSource

Simple wrapper on top of AudioSource, can be used for "regular" audio sources.
Will always use 3d spatial blending for playback!

### ProximityBasedAudioSource

An audio source that can be used to play back sounds with 2d spatial blending.
The audio volume and stereo panning will be calculated by the audio source's proximity to the screen center.

### NonSpatialAudioSource

A special audio source that will never take any spatial data into account and instead plays back sounds at a constant sound level.

## Player

The audio system contains several player implementations which provide generic audio functionality.
They are all implementing the singleton pattern and need to be added to your game scene if you want to use their functionality.

Prefabs for those players can be found in UnityTK/Assets/Audio/. Those prefabs are drag and drop ready, but their configurations can be changed.

### MusicPlayer

Music playback manager, used to play back MusicTrack audio events.

### AudioOneShotPlayer

Provides audio oneshot play capability for example for user interfaces.
The player is used by the component AudioOneShotOnClick, which plays an audio event on unity event system click events.
Additionally the player provides an api that can be used to implement custom behaviour.

### ProximityPlayer

Proximity player / manager behaviour needed in the scene in order to use ProximityBasedAudio.
Configuration can be tweaked to change how proximity is calculated.

## Utility

### AudioOneShotOnClick

Plays an audio event when a unity event system click event is recieved.
Requires AudioOneShotPlayer in the scene!