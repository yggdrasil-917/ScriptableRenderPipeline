# SRP Instance

An SRP Instance is the class that performs the rendering within an Scriptable Render Pipeline (SRP). At run time, the active SRP Asset creates and configures an SRP Instance. 

If your Project uses an SRP, then you must have an SRP Instance class in your Project and that SRP Instance must have a **Render** method. The code that you write within this method determines which actions Unity performs when rendering using your custom SRP. These actions might include:

* Clearing the framebuffer
* Performing Scene culling
* Rendering sets of GameObjects
* Doing blits from one frame buffer to another
* Rendering shadows
* Applying post-processing effects

The **Render** method takes takes two arguments:

* A `ScriptableRenderContext` which is a type of Command Buffer where you can enqueue rendering operations to be performed.
* A set of `Camera`s that to use for rendering.

## A basic pipeline
The SRP Asset example from [here](srp-asset.html) returns an SRP Instance, this pipeline might look like what is below. 

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
        // does not so much yet :()
        base.Render(context, cameras);

        // clear buffers to the configured color
        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, m_ClearColor);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
        context.Submit();
    }
}
```

What this pipeline does is perform a simple clear the screen to the given clear colour that is set in the SRP Asset when Unity creates the the SRP Instance. There are a few things to note here:

* SRP uses existing Unity `CommandBuffers` for many operations (`ClearRenderTarget` in this case).
* SRP schedules CommandBuffers against the context passed in.
* The final step of rendering in SRP is to call `Submit`. This executes all the queued up commands on the render context.

The `RenderPipeline`'s **Render** function is where you enter the rendering code for your custom renderer. It is here that you perform steps like Culling, Filtering, Changing render targets, and Drawing.