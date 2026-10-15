namespace NOQTA.Invoice.Application.Invoices.CancelInvoice;

public sealed record CancelInvoiceCommand(
    Guid InvoiceId);