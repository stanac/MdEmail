namespace MdEmail.Contracts;

public class SendEmailRequest
{
    /// <summary>
    /// Email To field
    /// </summary>
    public EmailAddressCollection To { get; internal set; } = new();

    /// <summary>
    /// Email CC field
    /// </summary>
    public EmailAddressCollection Cc { get; internal set; } = new();

    /// <summary>
    /// Email BCC field
    /// </summary>
    public EmailAddressCollection Bcc { get; internal set; } = new();

    /// <summary>
    /// Email subject
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// Email body
    /// </summary>
    public EmailBody Body { get; } = new();

    /// <summary>
    /// When set, value will be used for From name instead of value from configuration
    /// </summary>
    public string? OverrideDefaultFromName { get; set; }

    /// <summary>
    /// When set, value will be used for From email address instead of value from configuration
    /// </summary>
    public string? OverrideDefaultFromEmailAddress { get; set; }

    internal void EnsureValid()
    {
        Body.EnsureValid();

        List<string> addresses = To.Union(Cc).Union(Bcc).ToList();

        if (!addresses.Any())
        {
            throw new InvalidOperationException($"{nameof(SendEmailRequest)} not valid. No email address is set to {nameof(To)} or {nameof(Cc)} or {nameof(Bcc)}");
        }

        foreach (string s in addresses)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new InvalidOperationException($"{nameof(SendEmailRequest)} not valid. Email address set in {nameof(To)} or {nameof(Cc)} or {nameof(Bcc)} cannot be empty or whitespace.");
            }

            if (!EmailAddressValidator.IsValidEmailAddress(s, out string? error))
            {
                throw new InvalidOperationException($"{nameof(SendEmailRequest)} not valid. Value `{s}` is not valid email address. {error}");
            }
        }
    }
}
