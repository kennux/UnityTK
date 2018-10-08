# UnityTK

This is a library of scripts for Unity Engine Projects.
It contains several components almost every game project will need and can be used as a base-framework / toolbox for writing games.

# How to use

You can use UnityTK as git submodule:

`git submodule init`  
`git submodule add https://github.com/kennux/UnityTK`

In order to sync the UnityTK submodule code to your unity project the bash script in Utility/update_unitytk.sh can be used.

# Modules

DataBindings
-----

DataBindings provide a simple and very comfortable way to bind data to arbitrary objects (like UI).
They are employing a tree-like structure that lets you bind to specific field / properties of objects and / or invoke methods on objects inside your tree.

They are most commonly used in some MVVM-like structure.

BehaviourModel
----

Provides several pre-built components that can be used to create abstract behaviour models and employ a modular component based architecture.

Audio
----

The UnityTK audio system provides a very simple and lightweight abstraction layer on top of the unity engine audio system.
It provides the ability to construct game sound systems using events.

Cameras
----

The UnityTK camera system provides an abstract interface to generic camera mode implementations.

BuildSystem
----

Provides very simple interface and pre-built components to automate building unity projects.

Benchmarking
----

Simplistic (micro-)benchmarking system you can use to quickly author benchmarks and execute them in the unity editor.

Utility
-----

Provides utility for UnityTK itself and commonly used methods that are handy when working with unity.

# Deprecated / Obsolete modules

These modules should not be used in new projects anymore and are only still in the repository for old projects / reference.

AssetManagement
----

Asset bundle based asset management system that can be used to implement DLCs, Modding or just to seperate your assets into several asset bundles.
It provides an abstract and easy to use api to query for assets previously loaded and registered.
*DEPRECATED*: Unity's Addressables do the same thing, they are a lot better optimized and should be used instead of this: https://forum.unity.com/threads/addressables-are-here.536304/

