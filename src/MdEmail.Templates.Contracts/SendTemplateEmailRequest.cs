using MdEmail.Contracts;

namespace MdEmail.Templates.Contracts;

public class SendTemplateEmailRequest
{
    /// <summary>
    /// Optional tenant key. If not set, <see cref="TenantDefaults.DefaultTenantKey"/> is used.
    /// </summary>
    public string? TenantKey { get; init; }

    /// <summary>
    /// Template key
    /// </summary>
    public required string TemplateKey { get; init; }

    /// <summary>
    /// If true and template with <see cref="TemplateKey"/> and <see cref="TenantKey"/> is not found,
    /// fallback will be template with same <see cref="TemplateKey"/> and default <see cref="TenantKey"/>
    /// which is <see cref="TenantDefaults.DefaultTenantKey"/>.
    /// </summary>
    public bool FallbackToDefaultTenantTemplate { get; init; }

    /// <summary>
    /// When set and <see cref="FallbackToDefaultTenantTemplate"/> fails,
    /// this value will be used as key to find fallback template, first
    /// with <see cref="TenantKey"/> if specified, and if not, then <see cref="TenantDefaults.DefaultTenantKey"/>
    /// will be used.
    /// </summary>
    public string? FallbackTemplateKey { get; init; }

    /// <summary>
    /// Email To field
    /// </summary>
    public EmailAddressCollection To { get; } = new();
    
    /// <summary>
    /// Email CC field
    /// </summary>
    public EmailAddressCollection Cc { get; } = new();

    /// <summary>
    /// Email BCC field
    /// </summary>
    public EmailAddressCollection Bcc { get; } = new();

    /// <summary>
    /// When set this subject value will be used instead of subject set in template
    /// </summary>
    public string? OverrideTemplateSubject { get; set; }

    /// <summary>
    /// When set, value will be used for From name instead of value from configuration
    /// </summary>
    public string? OverrideDefaultFromName { get; set; }

    /// <summary>
    /// When set, value will be used for From email address instead of value from configuration
    /// </summary>
    public string? OverrideDefaultFromEmailAddress { get; set; }

    /// <summary>
    /// Data for template rendering (e.g. view data for razor template)
    /// </summary>
    public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

    internal void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(TemplateKey))
        {
            throw new InvalidOperationException($"{nameof(SendTemplateEmailRequest)} not valid. {nameof(TemplateKey)} must be specified.");
        }

        List<string> addresses = To.Union(Cc).Union(Bcc).ToList();

        if (!addresses.Any())
        {
            throw new InvalidOperationException($"{nameof(SendTemplateEmailRequest)} not valid. No email address is set to {nameof(To)} or {nameof(Cc)} or {nameof(Bcc)}");
        }

        foreach (string s in addresses)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new InvalidOperationException($"{nameof(SendTemplateEmailRequest)} not valid. Email address set in {nameof(To)} or {nameof(Cc)} or {nameof(Bcc)} cannot be empty or whitespace.");
            }

            if (!EmailAddressValidator.IsValidEmailAddress(s, out string? error))
            {
                throw new InvalidOperationException($"{nameof(SendTemplateEmailRequest)} not valid. Value `{s}` is not valid email address. {error}");
            }
        }
    }
}