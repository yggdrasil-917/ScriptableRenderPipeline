# Render Pipeline Asset
A Render Pipeline Asset, also known as an SRP Asset, is a [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) that represents a specific configuration for a Scriptable Render Pipeline (SRP). It stores information such as:

* What type of [Render Pipeline Instance](SRP-Instance) to use for rendering
* Whether GameObjects should cast shadows
* What Shader quality level to use
* The shadow distance
* The default Material configuration

You can also use a Render Pipeline Asset to provide a number of helper functions for things like:

- Default Material to use when creating 3d GameObjects
- Default Material to use when creating 2d GameObjects
- Default Material to use when creating Particle Systems
- Default Material to use when creating Terrain

## Run time functionality

At run time, before the first frame is rendered, Unity calls `InternalCreatePipeline` on the Render Pipeline Asset. The Render Pipeline Asset then returns a [Render Pipeline Instance](srp-instance) that is configured according to the Render Pipeline Asset's settings. Unity then uses that Render Pipeline Instance to perform rendering operations.

If a setting on the Render Pipeline Asset changes, Unity destroys the current Render Pipeline Instance and creates a new Render Pipeline instance with updated settings to use for the next frame.

## Setting a Render Pipeline Asset

You set your Project's Render Pipeline Asset in the [Graphics Settings](https://docs.unity3d.com/Manual/class-GraphicsSettings.html) window. When you set a Render Pipeline Asset, Unity uses SRP rendering in your Project with the configuration that the Render Pipeline Asset provides.

## Creating a Render Pipeline Asset

If your Project uses an SRP, then you must have an Render Pipeline Asset in your Project. The Render Pipeline Asset's class must inherit from `RenderPipelineAsset`.

You can override the **InternalCreatePipeline()** method to return a given type of [Render Pipeline Instance](SRP-Instance).

## Render Pipeline Asset example code
The example below shows an Render Pipeline Asset class. The Render Pipeline Asset defines a color that the [Render Pipeline Instance](SRP-Instance.md) uses to clear the screen.

There is also some Editor-only code that creates the Render Pipeline Asset in the Project.

```C#
[ExecuteInEditMode]
public class BasicAssetPipe : RenderPipelineAsset
{
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
        return new BasicPipeInstance(clearColor);
    }
}
```
