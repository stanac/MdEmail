using MailKit.Net.Smtp;
using Markdig;
using MdEmail.Contracts;
using MimeKit;

namespace MdEmail;

public class MdEmailSender : IMdEmailSender
{
    private readonly MdEmailConfiguration _config;

    public MdEmailSender(MdEmailConfiguration config)
    {
        config.EnsureValid();
        _config = config;
    }

    public Task SendAsync(SendEmailRequest request) => SendAsync(request, CancellationToken.None);

    public async Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        request.EnsureValid();

        MimeMessage msg = CreateMessage(request);
        using ISmtpClient client = await CreateAndConnectClientAsync();

        await client.SendAsync(msg, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private MimeMessage CreateMessage(SendEmailRequest request)
    {
        MimeMessage msg = new MimeMessage
        {
            Subject = request.Subject,
            Body = BuildBody(request)
        };

        SetToCcAndBcc(msg, request);
        SetFrom(msg, request);

        return msg;
    }

    private void SetToCcAndBcc(MimeMessage msg, SendEmailRequest request)
    {
        if (request.To.Any())
        {
            msg.To.AddRange(request.To.Select(x => new MailboxAddress(x, x)));
        }

        if (request.Cc.Any())
        {
            msg.Cc.AddRange(request.Cc.Select(x => new MailboxAddress(x, x)));
        }

        if (request.Bcc.Any())
        {
            msg.Bcc.AddRange(request.Bcc.Select(x => new MailboxAddress(x, x)));
        }
    }

    private void SetFrom(MimeMessage msg, SendEmailRequest request)
    {
        string? fromName = _config.FromName;
        string fromEmailAddress = _config.FromEmailAddress;

        if (request.OverrideDefaultFromEmailAddress is not null)
        {
            fromEmailAddress = request.OverrideDefaultFromEmailAddress;
        }

        if (request.OverrideDefaultFromName is not null)
        {
            fromName = request.OverrideDefaultFromName;
        }

        msg.From.Add(new MailboxAddress(fromName ?? fromEmailAddress, fromEmailAddress));
    }

    private MimeEntity BuildBody(SendEmailRequest request)
    {
        BodyBuilder body = new();

        if (request.Body.IsTextOnly)
        {
            body.TextBody = request.Body.TextBody;
        }
        else if (request.Body.HtmlBody is not null)
        {
            body.HtmlBody = request.Body.HtmlBody;
        }
        else if (request.Body.MdBody is not null)
        {
            MarkdownPipeline? pipeline = _config.MarkdigPipelineFactory();

            body.HtmlBody = Markdown.ToHtml(request.Body.MdBody, pipeline);
            body.TextBody = Markdown.ToPlainText(request.Body.MdBody, pipeline);
        }
        else
        {
            body.HtmlBody = request.Body.HtmlBody;
            body.TextBody = request.Body.TextBody;
        }

        return body.ToMessageBody();
    }

    private async Task<ISmtpClient> CreateAndConnectClientAsync()
    {
        SmtpClient client = new SmtpClient();
        await client.ConnectAsync(_config.ServerHost, _config.ServerPort, _config.UseEncryption);

        if (!_config.UseDefaultCredentials)
        {
            await client.AuthenticateAsync(_config.ServerUsername, _config.ServerPassword);
        }

        return client;
    }
}

public interface IMdEmailSender
{
    Task SendAsync(SendEmailRequest request);
    Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken);
}