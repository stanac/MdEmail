using MdEmail;
using MdEmail.Templates;
using MdEmail.Templates.Contracts;
using MdEmail.Templates.Data;
using MdEmail.Templates.Data.Sqlite;
using MdEmail.Templates.Models;
using MdEmail.Templates.Renderer;
using MdEmail.Templates.Rendering.Razor;
using Microsoft.Extensions.Configuration;

const string RazorRendererKey = "razor";
const string TemplateKey = "template1";

// Create sender

ConfigurationBuilder cb = new ConfigurationBuilder();
cb.AddUserSecrets(typeof(Program).Assembly, optional: false);

MdEmailConfiguration config = cb.Build().GetSection("smtp").Get<MdEmailConfiguration>()
    ?? throw new InvalidOperationException("Failed to get config from secrets");

IMdEmailSender sender = new MdEmailSender(config);
ITemplateRepository templateRepo = new SqliteTemplateRepository("Data Source=emailTemplates.sqlite");
IRenderer renderer = new RazorRenderer();

IMdEmailTemplateSender templateSender = new MdEmailTemplateSender(templateRepo, sender);
templateSender.AddRenderer(RazorRendererKey, () => renderer);

// Register template

await templateRepo.UpsertAsync(new Template
{
    TemplateKey = TemplateKey,
    TenantKey = TenantDefaults.DefaultTenantKey,
    Renderer = RazorRendererKey,
    Subject = "Test subject",
    MdTemplate = @"

Hello @FirstName @LastName,

**Welcome** to `@AppName`

    "
});

// Send email from template

SendTemplateEmailRequest request = new SendTemplateEmailRequest
{
    TemplateKey = TemplateKey,
    Data =
    {
        {"FirstName", "John"},
        {"LastName", "John"},
        {"AppName", "SuperApp"}
    }
};

await templateSender.SendAsync(request);

Console.WriteLine("Done");
