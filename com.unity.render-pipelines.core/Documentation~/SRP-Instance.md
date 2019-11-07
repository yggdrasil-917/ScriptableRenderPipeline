# Render Pipeline Instance

A Render Pipeline Instance, also known as an SRP Instance, is the class that performs the rendering within a Scriptable Render Pipeline (SRP).

A Project's [Render Pipeline Asset](SRP-Asset) sets the type of Render Pipeline Instance that Unity uses to render that Project.

## Creating a RenderPipelineInstance

If your Project uses an SRP, then you must have a class that defines a Render Pipeline Instance in your Project. That class must inherit from `RenderPipeline`, and must have a **Render** method.

The code that you write within the **Render** method determines which actions Unity performs when rendering using your custom SRP. These actions might include:

* Clearing the framebuffer
* Performing Scene culling
* Rendering sets of GameObjects
* Doing blits from one frame buffer to another
* Rendering shadows
* Applying post-processing effects

The **Render** method takes takes two arguments:

* A [Scriptable Render Context](SRP-Context), where you can enqueue and execute rendering commands
* One or more `Cameras` to use for rendering

## Render Pipeline Instance example code
In this example, the Render Pipeline Instance clears the screen to a given colour. The Render Pipeline Asset sets the color when it creates the Render Pipline Instance.

```C#
public class BasicPipeInstance : RenderPipeline
{
    private Color m_ClearColor = Color.black;

    public BasicPipeInstance(Color clearColor)
    {
        m_ClearColor = clearColor;
    }

    public override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        base.Render(context, cameras);

        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, m_ClearColor);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
        context.Submit();
    }
}
```