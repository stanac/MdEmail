using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MdEmail.Contracts;

public static class EmailAddressValidator
{
    private const int MaxLength = 260;
    private const int MaxDotCount = 16;

    private static readonly Regex _emailRegex = new(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,25}$");

    public static bool IsValidEmailAddress(string emailAddress, [NotNullWhen(false)]out string? error)
    {
        if (emailAddress.Length > MaxLength)
        {
            error = $"Value cannot have more than {MaxLength} characters.";
            return false;
        }

        if (emailAddress.Count(c => c == '.') > MaxDotCount)
        {
            error = $"Value cannot have more than {MaxDotCount} dot ('.') characters.";
            return false;
        }

        if (!_emailRegex.IsMatch(emailAddress))
        {
            error = "Value is not valid email address.";
            return false;
        }

        error = null;
        return true;
    }
}