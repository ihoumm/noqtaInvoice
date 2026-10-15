namespace NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;

public sealed record GenerateInvoicePdfCommand(
    Guid InvoiceId);