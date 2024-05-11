using MdEmail.Templates.Renderer;

namespace MdEmail.Templates.Rendering.Razor;

public class RazorRenderer : IRenderer
{
    public bool UseSyncRendering => true;

    public string Render(string template, IDictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public Task<string> RenderAsync(string template, IDictionary<string, object> data, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}