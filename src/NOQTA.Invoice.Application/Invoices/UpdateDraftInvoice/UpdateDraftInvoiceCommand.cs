namespace NOQTA.Invoice.Application.Invoices.UpdateDraftInvoice;

public sealed record UpdateDraftInvoiceCommand(
    Guid InvoiceId,
    IReadOnlyList<UpdateDraftInvoiceItem> Items,
    decimal TaxRate,
    decimal DiscountAmount);

public sealed record UpdateDraftInvoiceItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice);