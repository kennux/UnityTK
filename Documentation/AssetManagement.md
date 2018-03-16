# AssetManagement

The asset management system provides a simple api to query for assets which were previously registered to the AssetManager.
The manager will seperate assets by the tags they are being tagged with (via the IManagedAsset interface).
Using this system has the advantage of providing an abstract api that can "under the hood" pull its data from any arbitrary source.

Those sources can be asset bundles, mod-loaded content, ...

However the standard implementation of the AssetManagement comes only with AssetBundle loading out of the box, everything else has to be implemented by the game.

## AssetManager

The main class, implements a singleton pattern for acessing the manager.
It provides the loading / registering and querying api.

## IManagedAsset

This interface needs to be implemented by any asset that will be managable.
GameObjects which should be managed need to have a component implementing this interface.

### ManagedGameObject

IManagedAsset implementation as GameObject component.

### ManagedScriptableObject

Base-class for ScriptableObjects implementing IManagedAsset.

## AssetLoader

Abstract implementation of an asset loader that can be extended to implement custom asset loaders (for example for loading content from a modding api).
The asset loaders will be able to override prefabs, the asset loaders are being ran in the order they are in the scene.
If an asset is loaded that has an identifier another loaded asset already has, the latest asset loaded with the identifier wins.
This way asset loaders can override assets loaded from others.

This base class has 2 default implementations:

### AssetBundleLoader

This component should be used to load asset bundle content.
It supports asset bundle emulation in-editor (by just registering all assets in the `AssetDatabase` assigned to any asset bundle) and can load all asset bundles from a specified path relative to the player binary in runtime.

### ResourcesLoader

This component can be used to load all project resources and register them to the AssetManager.
Will use unity's `UnityEngine.Resources` API to retrieve data.

# Modding

The AssetManagement system itself does not come with any modding abilities at all.
How modding is implemented is up to the game, the AssetManagement system just makes adding content to the game a lot easier via an abstract and generalized API.

The intended workflow for modding is compiling the game's main code outside of the unitynengine into a DLL referencing unity engine dlls.
The dll created by this compilation can be loaded into a plain project, assets can be created with the components from your game, and bundles can be exported.
The exported bundles can be loaded back into the game with all scripting intact!

An alternative approach would be to provide a mod-loader that parses mode content from XML/JSON/etc. and registers it to the AssetManager.
You are only limited by your imagination :-)

If the game uses the AssetManagement system to query for data mod content will automatically be available in the game.