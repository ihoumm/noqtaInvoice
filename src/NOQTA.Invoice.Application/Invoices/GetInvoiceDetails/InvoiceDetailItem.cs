namespace NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;

public sealed record InvoiceDetailItem(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Currency);