using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Customers;

public sealed record CustomerInvoiceSnapshot(
    string Name,
    EmailAddress Email,
    Address? Address);