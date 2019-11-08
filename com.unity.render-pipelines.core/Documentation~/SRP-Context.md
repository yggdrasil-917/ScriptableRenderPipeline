# Scriptable Render Context
Unity's Scriptable Render Pipeline (SRP) renders using delayed execution. You build up a list of rendering commands, and then you tell Unity to execute them. The object that you use to build up this list of commands is called a Scriptable Render Context.

In SRP, you can use commands from Unity's existing `CommandBuffers` (such as `ClearRenderTarget`), and some SRP-specific commands that are listed Scriptable Render Context API documentation. You add these commands to your Scriptable Render Context, and then call `Submit` on the Scriptable Render Context to execute the commands.

This example shows you how to use a Scriptable Render Context to clear a render target.

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

For more information about the Scriptable Render Context, see the [API documentation](https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html).