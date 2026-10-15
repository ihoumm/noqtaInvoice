namespace NOQTA.Invoice.Application.Invoices.GetInvoices;

public sealed record InvoiceListItem(
    Guid Id,
    string InvoiceNumber,
    string InvoiceType,
    string Status,
    string Currency,
    string CustomerName,
    decimal Total,
    DateTimeOffset CreatedAt);