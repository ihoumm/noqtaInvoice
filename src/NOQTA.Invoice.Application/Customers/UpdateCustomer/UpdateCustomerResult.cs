namespace NOQTA.Invoice.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerResult(
    Guid CustomerId,
    string Name);