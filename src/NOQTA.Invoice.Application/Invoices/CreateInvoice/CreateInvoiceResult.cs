namespace NOQTA.Invoice.Application.Invoices.CreateInvoice;

public sealed record CreateInvoiceResult(
    Guid InvoiceId,
    string InvoiceNumber);