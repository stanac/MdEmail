using MdEmail;
using MdEmail.Templates;
using MdEmail.Templates.Contracts;
using MdEmail.Templates.Data;
using MdEmail.Templates.Data.Sqlite;
using MdEmail.Templates.Models;
using MdEmail.Templates.Renderer;
using MdEmail.Templates.Rendering.Razor;
using Microsoft.Extensions.Configuration;

const string TemplateKey = "template1";

// Create config

ConfigurationBuilder cb = new ConfigurationBuilder();
cb.AddUserSecrets(typeof(Program).Assembly, optional: false);
IConfigurationRoot appConfig = cb.Build();

MdEmailConfiguration config = appConfig.GetSection("smtp").Get<MdEmailConfiguration>()
    ?? throw new InvalidOperationException("Failed to get config from secrets");

// Create senders

IMdEmailSender sender = new MdEmailSender(config);
ITemplateRepository templateRepo = new SqliteTemplateRepository("Data Source=emailTemplates.sqlite");
IRenderer renderer = new RazorRenderer();

IMdEmailTemplateSender templateSender = new MdEmailTemplateSender(templateRepo, sender);
templateSender.AddRenderer(RazorRenderer.Key, () => renderer);

// Register template

await templateRepo.UpsertAsync(new Template
{
    TemplateKey = TemplateKey,
    TenantKey = TenantDefaults.DefaultTenantKey,
    Renderer = RazorRenderer.Key,
    Subject = "Test subject",
    MdTemplate = @"

Hello @Model.FirstName @Model.LastName,

**Welcome** to `@Model.AppName`

    "
});

// Send email from template

string testRecipient = appConfig["testRecipient"]
    ?? throw new InvalidOperationException("Failed to get `testRecipient` from user secrets");

SendTemplateEmailRequest request = new SendTemplateEmailRequest
{
    TemplateKey = TemplateKey,
    To =
    {
        testRecipient
    },
    Data =
    {
        {"FirstName", "John"},
        {"LastName", "Doe"},
        {"AppName", "SuperApp"}
    }
};

await templateSender.SendAsync(request);

Console.WriteLine("Done");
