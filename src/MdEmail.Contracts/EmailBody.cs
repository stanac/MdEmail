namespace MdEmail.Contracts;

public class EmailBody
{
    internal string? HtmlBody { get; private set; }
    internal string? MdBody { get; private set; }
    internal string? TextBody { get; private set; }

    internal bool IsTextOnly => TextBody is not null && HtmlBody is null && MdBody is null;

    /// <summary>
    /// Clears all bodies set by <see cref="SetHtmlBody"/>, <see cref="SetMarkdownBody"/> and <see cref="SetTextBody"/>.
    /// </summary>
    public void Clear()
    {
        HtmlBody = null;
        MdBody = null;
        TextBody = null;
    }

    internal void EnsureValid()
    {
        if (HtmlBody is null && MdBody is null && TextBody is null)
        {
            throw new InvalidOperationException($"{nameof(EmailBody)} not set. Set at least empty string as body.");
        }
    }

    /// <summary>
    /// Setting Markdown body will set both HTML and Text body.
    /// To override Text body call <see cref="SetTextBody" /> after calling this method.
    /// </summary>
    /// <param name="markdownBody">Email body</param>
    public void SetMarkdownBody(string markdownBody)
    {
        MdBody = markdownBody;
    }

    /// <summary>
    /// Setting HTML body will not set Text body. Setting HTML body will ignore Markdown body.
    /// Call <see cref="SetTextBody"/> to set Text body, or call
    /// <see cref="SetMarkdownBody"/> which will set both HTML body and Text body.
    /// </summary>
    /// <param name="htmlBody">Email body</param>
    public void SetHtmlBody(string htmlBody)
    {
        HtmlBody = htmlBody;
    }

    /// <summary>
    /// This method will set only TextBody
    /// </summary>
    /// <param name="textBody">Email body</param>
    public void SetTextBody(string textBody)
    {
        TextBody = textBody;
    }
}