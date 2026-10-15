namespace NOQTA.Invoice.Domain.Invoices;

public readonly record struct InvoiceNumber
{
    public string Value { get; }

    private InvoiceNumber(string value)
    {
        Value = value;
    }

    public static InvoiceNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Invoice number is required.",
                nameof(value));

        return new InvoiceNumber(value.Trim());
    }

    public override string ToString() => Value;
}