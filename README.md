# MdEmail

Simple email sending library (over SMTP) with support for Markdown for .NET 8+. Depends on `Markdig` and `MailKit`.

---

## Install MdEmail

```
dotnet add package MdEmail
```

## Use MdEmail

```csharp
// appConfig is IConfiguration
MdEmailConfiguration config = appConfig.GetSection("smtp").Get<MdEmailConfiguration>();

IMdEmailSender sender = new MdEmailSender(config);

SendEmailRequest request = new SendEmailRequest
{
    Subject = "test subject"
};
request.To.Add("someone@example.com");

request.Body.SetMarkdownBody(@"**bold** *italic*

Test msg
");

await sender.SendAsync(request);
```

Config example:
```json
{
  "smtp": {
    "useEncryption": true,
    "useDefaultCredentials": false,
    "serverHost": "host.example.com",
    "serverUsername": "someone_username",
    "serverPassword": "someone_password",
    "serverPort": 465,
    "fromEmailAddress": "someone@example.com",
    "fromName": "Someone Someson"
  }
}
```

## MdEmail.Contracts

For code that is only composing and not sending emails use `MdEmail.Contracts` nuget package.

```
dotnet add package MdEmail.Contracts
```

This library contains following classes:

```
EmailAddressCollection
EmailAddressValidator
EmailBody
SendEmailRequest
```

---

# MdEmail.Templates

Templates for MdEmail with support for Postgres or Sqlite for starage and Razor for rendering.
Depends on `RazorLight`, `Dapper`, and either `Npgsql` or `Microsoft.Data.Sqlite`.

## Install MdEmail.Templates

Default implementation for data storage (Postgres or Sqlite) and template rendering (Razor)

Add core template library:

```
dotnet add package MdEmail.Templates
```

Add Razor rendering:

```
dotnet add package MdEmail.Templates.Rendering.Razor
```

Add storage

```
dotnet add package MdEmail.Templates.Data.Postgres
```
or 
```
dotnet add package MdEmail.Templates.Data.Sqlite
```

## MdEmail.Templates.Contracts

For code that is only composing and not sending emails use `MdEmail.Templates.Contracts` nuget package.

```
dotnet add package MdEmail.Contracts
```

This library contains only `SendTemplateEmailRequest` and `TenantDefaults` classes.

## Custom MdEmail.Templates data storage

To implement custom `ITemplateRepository` reference `MdEmail.Templates` and implement `MdEmail.Templates.Data.ITemplateRepository` interface.

Only single data storage can be used.

For simpler applications `MdEmail.Templates.Data.InMemoryTemplateRepository` can be used.

Register custom data storage using `MdEmailTemplateSender` constructor.

## Custom MdEmail.Templates renderer

To implement custom `IRenderer`, reference `MdEmail.Templates` and implement `MdEmail.Templates.Data.ITemplateRepository` interface.

Register custom renderer by calling `MdEmailTemplateSender.AddRenderer(string, Func<IRenderer>)`.
Multiple renderers can be registered.

## Use MdEmail.Templates

(this example is using Razor for rendering and Sqlite for storage)

```csharp
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
```