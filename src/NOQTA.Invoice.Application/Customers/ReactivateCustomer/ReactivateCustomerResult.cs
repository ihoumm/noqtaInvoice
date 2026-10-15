namespace NOQTA.Invoice.Application.Customers.ReactivateCustomer;

public sealed record ReactivateCustomerResult(
    Guid CustomerId,
    string Name);