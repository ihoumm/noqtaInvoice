using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Invoices;

public sealed record InvoiceTotals(
    Money Subtotal,
    Money DiscountAmount,
    Money TaxAmount,
    Money Total);