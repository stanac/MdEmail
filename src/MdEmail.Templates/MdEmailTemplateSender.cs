using Markdig;
using MdEmail.Contracts;
using MdEmail.Templates.Contracts;
using MdEmail.Templates.Data;
using MdEmail.Templates.Models;
using MdEmail.Templates.Renderer;
using System.Text;

namespace MdEmail.Templates;

public class MdEmailTemplateSender : IMdEmailTemplateSender
{
    private readonly ITemplateRepository _repo;
    private readonly IMdEmailSender _sender;
    private readonly MarkdownPipeline? _mdPipeline;
    private readonly Dictionary<string, Func<IRenderer>> _renderers = new();

    public MdEmailTemplateSender(ITemplateRepository repository, IMdEmailSender sender)
    {
        _repo = repository;
        _sender = sender;
        _mdPipeline = sender.GetMarkdownPipeline();
    }

    public void AddRenderer(string key, Func<IRenderer> rendererFactory)
    {
        _renderers[key] = rendererFactory;
    }

    public Task SendAsync(SendTemplateEmailRequest request) => SendAsync(request, CancellationToken.None);

    public async Task SendAsync(SendTemplateEmailRequest request, CancellationToken cancellationToken)
    {
        Template template = await GetTemplateAsync(request, cancellationToken);

        if (!_renderers.TryGetValue(template.Renderer, out var rendererFact))
        {
            throw new InvalidOperationException($"Renderer with key `{template.Renderer}` for template `{template.TemplateKey}` in tenant `{template.TenantKey}` isn't registered.");
        }

        IRenderer renderer = rendererFact();

        (string? html, string text) body = await RenderAsync(renderer, template, request, cancellationToken);

        SendEmailRequest sendRequest = new SendEmailRequest
        {
            Subject = request.OverrideTemplateSubject ?? template.Subject,
            To = request.To,
            Cc = request.Cc,
            Bcc = request.Bcc,
            OverrideDefaultFromEmailAddress = request.OverrideDefaultFromEmailAddress,
            OverrideDefaultFromName = request.OverrideDefaultFromName
        };

        sendRequest.Body.SetTextBody(body.text);

        if (body.html is not null)
        {
            sendRequest.Body.SetHtmlBody(body.html);
        }

        await _sender.SendAsync(sendRequest, cancellationToken);
    }

    private async Task<Template> GetTemplateAsync(SendTemplateEmailRequest request, CancellationToken cancellationToken)
    {
        string tenantKey = request.TenantKey ?? TenantDefaults.DefaultTenantKey;

        Template? template = await _repo.GetAsync(tenantKey, request.TemplateKey, cancellationToken);

        if (template is not null)
        {
            return template;
        }

        bool findInDefaultTenant = false;

        if (request.FallbackToDefaultTenantTemplate && tenantKey != TenantDefaults.DefaultTenantKey)
        {
            findInDefaultTenant = true;

            template = await _repo.GetAsync(TenantDefaults.DefaultTenantKey, request.TemplateKey, cancellationToken);

            if (template is not null)
            {
                return template;
            }
        }

        bool findFallback = false;

        if (request.FallbackTemplateKey is not null)
        {
            findFallback = true;

            template = await _repo.GetAsync(tenantKey, request.FallbackTemplateKey, cancellationToken);

            if (template is not null)
            {
                return template;
            }
        }

        bool findFallbackInDefault = false;

        if (request.FallbackToDefaultTenantTemplate && request.FallbackTemplateKey is not null && tenantKey != TenantDefaults.DefaultTenantKey)
        {
            findFallbackInDefault = true;

            template = await _repo.GetAsync(TenantDefaults.DefaultTenantKey, request.FallbackTemplateKey, cancellationToken);

            if (template is not null)
            {
                return template;
            }
        }

        StringBuilder sb = new();

        sb.Append($"Failed to find template. Following templates where searched for: (tenant: `{tenantKey}`, template: `{request.TemplateKey}`)");

        if (findInDefaultTenant)
        {
            sb.Append($", (tenant: `{TenantDefaults.DefaultTenantKey}`, template: `{request.TemplateKey}`)");
        }

        if (findFallback)
        {
            sb.Append($", (tenant: `{tenantKey}`, template: `{request.FallbackTemplateKey}`)");
        }

        if (findFallbackInDefault)
        {
            sb.Append($", (tenant: `{TenantDefaults.DefaultTenantKey}`, template: `{request.FallbackTemplateKey}`)");
        }

        sb.Append('.');

        string error = sb.ToString();
        sb.Clear();

        throw new InvalidOperationException(error);
    }

    private async Task<(string? html, string text)> RenderAsync(IRenderer renderer, Template template, SendTemplateEmailRequest request, CancellationToken cancellationToken)
    {
        if (template.MdTemplate is not null)
        {
            string htmlTemplate = Markdown.ToHtml(template.MdTemplate, _mdPipeline);
            string textTemplate = Markdown.ToPlainText(template.MdTemplate, _mdPipeline);

            string htmlBody = await RenderSyncOrAsync(renderer, htmlTemplate, request.Data, cancellationToken);
            string textBody = await RenderSyncOrAsync(renderer, textTemplate, request.Data, cancellationToken);

            return (htmlBody, textBody);
        }

        if (template.TextTemplate is null)
        {
            throw new InvalidOperationException(
                $"Failed to render email template (tenant: `{template.TenantKey}`, template: {template.TemplateKey}) " +
                $"{nameof(template.TextTemplate)} must be set when {nameof(template.MdTemplate)} is not set."
            );
        }

        string text = await RenderSyncOrAsync(renderer, template.TextTemplate, request.Data, cancellationToken);
        string? html = null;

        if (template.HtmlTemplate is not null)
        {
            html = await RenderSyncOrAsync(renderer, template.HtmlTemplate, request.Data, cancellationToken);
        }

        return (html, text);
    }

    private async Task<string> RenderSyncOrAsync(IRenderer renderer, string template, IDictionary<string, object> data, CancellationToken cancellationToken)
    {
        if (renderer.UseSyncRendering)
        {
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            return renderer.Render(template, data);
        }

        return await renderer.RenderAsync(template, data, cancellationToken);
    }
}