using Markdig;
using MdEmail.Contracts;

namespace MdEmail;

public interface IMdEmailSender
{
    Task SendAsync(SendEmailRequest request);
    Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken);
    MarkdownPipeline? GetMarkdownPipeline();
}