using Markdig;
using MdEmail.Contracts;

namespace MdEmail;

public class MdEmailConfiguration
{
    private bool _validated;

    public required string? FromName { get; init; }
    public required string FromEmailAddress { get; init; }
    public required bool UseDefaultCredentials { get; init; }
    public required bool UseEncryption { get; init; }
    public required string ServerHost { get; init; }
    public required int ServerPort { get; init; }
    public required string? ServerUsername { get; init; }
    public required string? ServerPassword { get; init; }

    public Func<MarkdownPipeline?> MarkdigPipelineFactory { get; set; } = () => null;

    internal void EnsureValid()
    {
        if (_validated)
        {
            return;
        }

        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(FromEmailAddress))
        {
            errors.Add($"{nameof(FromEmailAddress)} must be set.");
        }
        else if (!EmailAddressValidator.IsValidEmailAddress(FromEmailAddress, out string? emailAddressError))
        {
            errors.Add($"{nameof(FromEmailAddress)}: {emailAddressError}");
        }

        if (!UseDefaultCredentials)
        {
            if (string.IsNullOrWhiteSpace(ServerUsername))
            {
                errors.Add($"{nameof(ServerUsername)} must be set when {nameof(UseDefaultCredentials)} is true.");
            }

            if (string.IsNullOrWhiteSpace(ServerPassword))
            {
                errors.Add($"{nameof(ServerPassword)} must be set when {nameof(UseDefaultCredentials)} is true.");
            }
        }

        if (string.IsNullOrWhiteSpace(ServerHost))
        {
            errors.Add($"{nameof(ServerHost)} must be set.");
        }

        if (ServerPort < 1)
        {
            errors.Add($"{nameof(ServerPort)} must be greater than 0.");
        }

        if (errors.Any())
        {
            string allErrors = string.Join(" ", errors);

            allErrors = $"{nameof(MdEmailConfiguration)} not valid. {allErrors}";

            throw new InvalidOperationException(allErrors);
        }

        _validated = true;
    }
}