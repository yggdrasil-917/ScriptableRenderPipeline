# Scriptable Render Context
A Scriptable Render Pipeline (SRP) uses the concept of delayed execution: you build up a list of rendering commands, and then you execute them. The object that Unity uses to store and execute these commands is called a Scriptable Render Context.

SRP uses a mix of existing Unity `CommandBuffers` (such as `ClearRenderTarget`) and SRP-specific commands. Once you have enqueued your commands, you must call `Submit` execute them.

In this example, a `CommandBuffer` clears a render target.

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

        // clear buffers to the configured color
        var cmd = new CommandBuffer();
        cmd.ClearRenderTarget(true, true, m_ClearColor);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
        context.Submit();
    }
}
```

For more information about the Scriptable Render Context, see the [API documentation](https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html).