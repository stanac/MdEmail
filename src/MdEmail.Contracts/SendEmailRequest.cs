namespace MdEmail.Contracts;

public class SendEmailRequest
{
    public EmailAddressCollection To { get; } = new();
    public EmailAddressCollection Cc { get; } = new();
    public EmailAddressCollection Bcc { get; } = new();

    public required string Subject { get; set; }
    public EmailBody Body { get; } = new();

    public string? OverrideDefaultFromName { get; set; }
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
