using MdEmail.Templates.Contracts;
using MdEmail.Templates.Renderer;

namespace MdEmail.Templates;

public interface IMdEmailTemplateSender
{
    void AddRenderer(string key, Func<IRenderer> rendererFactory);
    Task SendAsync(SendTemplateEmailRequest request);
    Task SendAsync(SendTemplateEmailRequest request, CancellationToken cancellationToken);
}