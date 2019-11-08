# Render Pipeline Instance

A Render Pipeline Instance is the class that performs the rendering within a Scriptable Render Pipeline (SRP).

A Project's [Render Pipeline Asset](SRP-Asset) sets the type of Render Pipeline Instance that Unity uses to render that Project.

## Creating a Render Pipeline Instance

If your Project uses an SRP, you must have a class that defines a Render Pipeline Instance in your Project. That class must inherit from `RenderPipeline`, and must have a `Render` method.

The code that you write within the **Render** method determines which rendering commands Unity uses to render the Scene. For example, these commands could:

* Clearg the framebuffer
* Perform Scene culling
* Render sets of GameObjects
*  Blit pixels from one frame buffer to another
* Renderg shadows
* Apply post-processing effects

The **Render** method takes takes two arguments:

* A [Scriptable Render Context](SRP-Context), where you add commands to the render queue and execute those commands.
* One or more `Cameras` to use for rendering.

## Render Pipeline Instance example code
In this example, the Render Pipeline Instance clears the screen to a given colour.

```C#
public class BasicPipeInstance : RenderPipeline
{
    private Color m_ClearColor = Color.black;

    public BasicPipeInstance(Color clearColor)
    {
        // the configured color is passed in via the constructor, when the Render Pipeline Asset creates this instance
        m_ClearColor = clearColor;
    }

    public override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        base.Render(context, cameras);

        // uses a CommandBuffer to clear buffers to the configured color
        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, m_ClearColor);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
        context.Submit();
    }
}
```