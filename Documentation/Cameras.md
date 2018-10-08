# Cameras

Provides an abstract foundation / framework for implementing arbitrary generic camera modes.
Also some pre-implemented camera modes.

## Base

### UTKCamera

UnityTK camera mono behaviour.
This camera manages multiple cameramodes and provides an interface to access and manipulate it.

### CameraMode

Abstract base class for implementing camera modes.
Camera modes define the behaviour of a camera based on a set of input parameters (the input data).

### CameraModeInput

An input module for camera modes.
Every camera mode defines a base class for implementing inputs for the mode.
UnityTK itself comes with desktop input implementations only.

### ICameraModeFeature

Interface to be used as base for implementing camera features.
These interfaces can be implemented specifically for a camera mode or just be shared between modes.
They encapsulate an api a camera must provide in order to be able to use the feature.

For actually implementing features as unity mono behaviours use the base class CameraModeFeatureBase and extend it with the interface of the feature.

## Implementations

### FreeLook

A simple freelook camera mode to be used for cameras that are able to look around in any direction.
Most common use-cases of this type of camera are FPS.

### TopDown

A top down camera implementation that is able to be used for regular top down cameras aswell as isometric cameras.
Essentially it can be used for any kind of "overhead" camera.