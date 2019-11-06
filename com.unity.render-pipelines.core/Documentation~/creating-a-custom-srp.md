# Creating a custom Scriptable Render Pipeline

Unity provides two prebuilt Scriptable Render Pipelines (SRPs): the [Universal Render Pipeline (URP)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest), and the [High Definition Render Pipeline (HDRP)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest). URP and HDRP offer extensive customization options to help you achieve the graphics and the performance you need, and should be suitable for most use cases. However, if you want even more control over your rendering pipeline using C# scripts, you can create your own custom SRP.

You can create a custom SRP from scratch, or you can create a custom SRP based on one of the prebuilt SRPs. Whichever you choose, the process you must follow to get started is broadly the same: you obtain a copy of the SRP source code, update your Unity Project's package settings to point to the SRP source code, and then modify the source code as required.

The SRP source code is hosted in a GitHub repository located at  [https://github.com/Unity-Technologies/ScriptableRenderPipeline](https://github.com/Unity-Technologies/ScriptableRenderPipeline). To make a copy of the source code, clone this repository using Git. For information on how to do this, see the GitHub help on [Cloning a repository](https://help.github.com/en/github/creating-cloning-and-archiving-repositories/cloning-a-repository).

## Creating a custom SRP from scratch

1. Create a new Unity Project.
2. Use Git to create a clone of the SRP source code repository somewhere on your machine. You can place the SRP source code inside your Unity Project, as long as it is not in one of the [reserved Project sub-folders](https://docs.unity3d.com/Manual/upm-ui-local.html#PkgLocation).
3. Open your Project in Unity, and then open the Package Manager window.
4. In the top left of the Package Manager window, click the plus (+) icon and then click "Add package from disk".
5. In the file browser, navigate to where you placed the clone of the SRP repository, open the folder called _com.unity.render-pipelines.core_, and double click the file called _package.json_. Unity adds a local copy of the SRP Core package to the packages list.
6. Optional: if you want to modify the Shader Graph code as part of your custom SRP, repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.shadergraph_.
7. Optional: if you want to modify the Visual Effect Graph code as part of your custom SRP, repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.visualeffectgraph_.
8. You can now debug and modify the scripts in your copy of the SRP source code, and see the results of your changes in your Unity Project.

## Creating a custom SRP based on URP 

1. Create a new Unity Project.
2. Use Git to create a clone of the SRP source code repository somewhere on your machine. You can place the SRP source code inside your Unity Project, as long as it is not in one of the [reserved Project sub-folders](https://docs.unity3d.com/Manual/upm-ui-local.html#PkgLocation).
3. Open your Project in Unity, and then open the Package Manager window.
4. In the top left of the Package Manager window, click the plus (+) icon and then click "Add package from disk".
5. In the file browser, navigate to where you placed the clone of the SRP repository, open the folder called _com.unity.render-pipelines.core_, and double click the file called _package.json_. Unity adds a local copy of the SRP Core package to the packages list.
6. Repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.shadergraph_.
7. Optional: if you want to modify the Visual Effect Graph code as part of your custom SRP, repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.visualeffectgraph_.
8. Repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.universal_.
9. You can now debug and modify the scripts in your copy of the SRP source code, and see the results of your changes in your Unity Project.

## Creating a custom SRP based on HDRP
1. Create a new Unity Project.
2. Use Git to create a clone of the SRP source code repository somewhere on your machine. You can place the SRP source code inside your Unity Project, as long as it is not in one of the [reserved Project sub-folders](https://docs.unity3d.com/Manual/upm-ui-local.html#PkgLocation).
3. Open your Project in Unity, and then open the Package Manager window.
4. In the top left of the Package Manager window, click the plus (+) icon and then click "Add package from disk".
5. In the file browser, navigate to where you placed the clone of the SRP repository, open the folder called _com.unity.render-pipelines.core_, and double click the file called _package.json_. Unity adds a local copy of the SRP Core package to the packages list.
6. Repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.shadergraph_.
7. Repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.visualeffectgraph_.
8. Repeat steps 4 and 5 for the folder named _com.unity.render-pipelines.high-definition_.
9. You can now debug and modify the scripts in your copy of the SRP source code, and see the results of your changes in your Unity Project.