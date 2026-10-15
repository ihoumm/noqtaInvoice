namespace NOQTA.Invoice.Domain.ValueObjects;

public readonly record struct Currency
{
    public string Code { get; }

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency MAD => new("MAD");

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
            throw new ArgumentException(
                "Currency code must contain exactly three characters.",
                nameof(code));

        return new Currency(normalized);
    }

    public override string ToString() => Code;
}