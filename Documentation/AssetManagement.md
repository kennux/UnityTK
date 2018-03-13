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

## AssetBundleLoader

This component should be used to load asset bundle content.
It supports asset bundle emulation in-editor (by just registering all assets in the `AssetDatabase` assigned to any asset bundle) and can load all asset bundles from a specified path relative to the player binary in runtime.

# Modding

The AssetManagement system itself does not come with any modding abilities at all.
How modding is implemented is up to the game, the AssetManagement system just makes adding content to the game a lot easier via an abstract and generalized API.

The intended workflow for modding is compiling the game's main code outside of the unitynengine into a DLL referencing unity engine dlls.
The dll created by this compilation can be loaded into a plain project, assets can be created with the components from your game, and bundles can be exported.
The exported bundles can be loaded back into the game with all scripting intact!

If the game uses the AssetManagement system to query for data mod content will automatically be available in the game.