# SRP Asset
An SRP Asset is a [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) that represents a specific configuration for a Scriptable Render Pipeline (SRP). It stores information such as:

* Whether GameObjects should cast shadows
* What Shader quality level to use
* The shadow distance
* The default Material configuration

When you set a reference to an SRP Asset in GraphicsSettings, Unity uses SRP rendering in your Project with the configuration that the SRP Asset provides.

At run time, Unity calls `InternalCreatePipeline` on the SRP Asset. The SRP Asset returns an [SRP Instance](srp-instance) that is configured according to the SRP Asset's settings.

You can also use the SRP Asset to provide a number of helper functions for things like:

- Default Material to use when creating 3d GameObjects
- Default Material to use when creating 2d GameObjects
- Default Material to use when creating Particle Systems
- Default Material to use when creating Terrain

## An SRP Asset example
The Asset contains rendering properties and returns an instance of a pipeline that Unity can use to render your Scene. If a setting on the Asset changes, Unity destroys all current instances and creates a new instance with the new settings to use for the next frame.

The example below shows an SRP Asset class. It contains a color that the [SRP Instance](srp-instance.md) uses to clear the screen. There is also some editor only code that assists the user in creating an SRP Asset in the Project. This is important as you need to set this Asset in the graphics settings window.

```C#
[ExecuteInEditMode]
public class BasicAssetPipe : RenderPipelineAsset
{
    public Color clearColor = Color.green;

#if UNITY_EDITOR
    // Call to create a simple pipeline
    [UnityEditor.MenuItem("SRP-Demo/01 - Create Basic Asset Pipeline")]
    static void CreateBasicAssetPipeline()
    {
        var instance = ScriptableObject.CreateInstance<BasicAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/BasicAssetPipe.asset");
    }
#endif

    // Function to return an instance of this pipeline
    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new BasicPipeInstance(clearColor);
    }
}
```
