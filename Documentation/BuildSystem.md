# BuildSystem

Provides a simplistic api to automate building unity projects.
The BuildSystem can create BuildJobs which composed of multiple BuildTasks.

## BuildJob

A job that can be executed by the build system which is composed of multiple BuildTask implementations.
Jobs are always built into a specific directory, while tasks can modify subfolders and files.

## BuildTask

Abstract base class for implementing build tasks

### BuildPlayerTask

Task that can be used to build a player with a specific set of build parameters into a subfolder.

### BuildAssetBundlesTask

Task that can be used to build all AssetBundles, defined by the user in the editor, into a subfolder of the job destination.