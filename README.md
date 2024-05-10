# MdEmail

Simple email sending library (over STMP) with support for Markdown for .NET 8+. Depends on `Markdig` and `MailKit`.

---

## Install

```
dotnet add package MdEmail
```

## Use

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
