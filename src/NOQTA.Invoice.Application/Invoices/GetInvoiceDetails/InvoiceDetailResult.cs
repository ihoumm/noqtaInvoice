namespace NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;

public sealed record InvoiceDetailResult(
    Guid Id,
    string InvoiceNumber,
    string InvoiceType,
    string Status,
    string Currency,
    string CustomerName,
    string CustomerEmail,
    InvoiceDetailAddress? CustomerAddress,
    decimal TaxRate,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? PaidAt,
    IReadOnlyList<InvoiceDetailItem> Items);

public sealed record InvoiceDetailAddress(
    string Street,
    string City,
    string PostalCode,
    string Country);