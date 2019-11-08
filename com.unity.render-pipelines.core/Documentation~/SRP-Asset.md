# Render Pipeline Asset

A Render Pipeline Asset is a [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) that represents a specific configuration for a Scriptable Render Pipeline (SRP). It stores information such as:

* Which type of [Render Pipeline Instance](SRP-Instance) to use for rendering
* Whether GameObjects should cast shadows
* Which Shader quality level to use
* The shadow distance
* The default Material configuration

You can also use a Render Pipeline Asset to provide helper functions for things like the default Material to use when creating  3D GameObjects, 2D GameObjects, Particle Systems, and Terrain.

## Run time functionality

At run time, Unity calls `InternalCreatePipeline` on the Render Pipeline Asset before it renders the first frame, . The Render Pipeline Asset then returns a [Render Pipeline Instance](srp-instance) that is configured according to the Render Pipeline Asset's settings. Unity then uses that Render Pipeline Instance to render the Scene.

If a setting on the Render Pipeline Asset changes, Unity destroys the current Render Pipeline Instance and creates another Render Pipeline Instance with updated settings to use for the next frame.

## Setting a Render Pipeline Asset

You set your Project's Render Pipeline Asset in the [Graphics Settings](https://docs.unity3d.com/Manual/class-GraphicsSettings.html) window. When you set a Render Pipeline Asset, Unity uses SRP rendering in your Project with the configuration that the Render Pipeline Asset provides.

## Creating a Render Pipeline Asset

If your Project uses an SRP, you must have a Render Pipeline Asset in your Project. The Render Pipeline Asset's class must inherit from `RenderPipelineAsset`.

You can override the `InternalCreatePipeline()` method to return a given type of [Render Pipeline Instance](SRP-Instance).

## Render Pipeline Asset example code
The example below shows a Render Pipeline Asset class. The Render Pipeline Asset defines a color that the [Render Pipeline Instance](SRP-Instance.md) uses to clear the screen.

There is also some Editor-only code that creates the Render Pipeline Asset in the Project.

```C#
[ExecuteInEditMode]
public class BasicAssetPipe : RenderPipelineAsset
{
    // define the color that will be passed to the Render Pipeline Instance
    public Color clearColor = Color.green;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("SRP-Demo/01 - Create Basic Asset Pipeline")]
    static void CreateBasicAssetPipeline()
    {
        var instance = ScriptableObject.CreateInstance<BasicAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/BasicAssetPipe.asset");
    }
#endif

    protected override IRenderPipeline InternalCreatePipeline()
    {
        // Instantiate a Render Pipeline Instance of the type BasicPipelineInstance, and pass in the color definied above
        return new BasicPipeInstance(clearColor);
    }
}
```