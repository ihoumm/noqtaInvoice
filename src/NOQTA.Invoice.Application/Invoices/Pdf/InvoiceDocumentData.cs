namespace NOQTA.Invoice.Application.Invoices.Pdf;

public sealed record InvoiceDocumentData(
    CompanyDocumentData Company,
    InvoiceDocumentInfo Invoice,
    CustomerDocumentData Customer,
    IReadOnlyList<InvoiceDocumentItem> Items,
    InvoiceDocumentTotals Totals);

public sealed record CompanyDocumentData(
    string LegalName,
    string Email,
    string Phone,
    string Street,
    string City,
    string PostalCode,
    string Country,
    string? TaxIdentifier,
    string? LogoPath);

public sealed record InvoiceDocumentInfo(
    string InvoiceNumber,
    string InvoiceType,
    string Status,
    string Currency,
    DateTimeOffset CreatedAt,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? PaidAt);

public sealed record CustomerDocumentData(
    string Name,
    string Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);

public sealed record InvoiceDocumentItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Currency);

public sealed record InvoiceDocumentTotals(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal Total,
    string Currency);