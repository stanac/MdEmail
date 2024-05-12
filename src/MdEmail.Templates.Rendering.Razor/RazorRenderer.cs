using System.Dynamic;
using MdEmail.Templates.Renderer;
using RazorLight;

namespace MdEmail.Templates.Rendering.Razor;

public class RazorRenderer : IRenderer
{
    public const string Key = "Razor";

    public bool UseSyncRendering => false;

    public string Render(string template, IDictionary<string, object> data)
    {
        return RenderAsync(template, data, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<string> RenderAsync(string template, IDictionary<string, object> data, CancellationToken cancellationToken)
    {
        RazorLightEngine engine = new RazorLightEngineBuilder()
            .UseNoProject()
            .AddDynamicTemplates(new Dictionary<string, string>
            {
                {"1", template}
            })
            .Build();

        dynamic model = new ExpandoObject();

        foreach (KeyValuePair<string, object> pair in data)
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            ((IDictionary<string, object>)(ExpandoObject)model)[pair.Key] = pair.Value;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        
        return await engine.CompileRenderStringAsync(Guid.NewGuid().ToString(), template, model);
    }
}