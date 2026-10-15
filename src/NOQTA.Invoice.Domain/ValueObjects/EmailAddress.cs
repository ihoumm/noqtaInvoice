using System.Net.Mail;

namespace NOQTA.Invoice.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address is required.", nameof(value));

        var normalized = value.Trim();

        try
        {
            var address = new MailAddress(normalized);

            if (!string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase))
                throw new FormatException();

            return new EmailAddress(normalized);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "Invalid email address.",
                nameof(value));
        }
    }

    public override string ToString() => Value;
}