namespace NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;

public sealed record GenerateInvoicePdfResult(
    Guid InvoiceId,
    string InvoiceNumber,
    string FilePath);