using System.Collections;

namespace MdEmail.Contracts;

/// <summary>
/// Collection of email addresses
/// </summary>
public class EmailAddressCollection : IEnumerable<string>
{
    private readonly ISet<string> _addresses = new HashSet<string>();

    public bool Add(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(emailAddress));
        }

        if (!EmailAddressValidator.IsValidEmailAddress(emailAddress, out string? error))
        {
            throw new ArgumentException(error, nameof(emailAddress));
        }

        return _addresses.Add(emailAddress);
    }

    public bool Remove(string emailAddress)
    {
        return _addresses.Remove(emailAddress);
    }

    public void Clear() => _addresses.Clear();

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string a in _addresses)
        {
            yield return a;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}