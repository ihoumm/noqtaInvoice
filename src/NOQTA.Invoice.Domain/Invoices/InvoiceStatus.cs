namespace NOQTA.Invoice.Domain.Invoices;

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Sent = 3,
    PartiallyPaid = 4,
    Paid = 5,
    Overdue = 6,
    Cancelled = 7
}