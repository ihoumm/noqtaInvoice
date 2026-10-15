namespace NOQTA.Invoice.Application.Invoices.CreateInvoice;

public sealed record CreateInvoiceCommand(
    Guid CustomerId,
    string InvoiceType,
    string CurrencyCode,
    IReadOnlyList<CreateInvoiceItem> Items);

public sealed record CreateInvoiceItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice);