namespace NOQTA.Invoice.Application.Customers.CreateCustomer;

public sealed record CreateCustomerResult(
    Guid CustomerId,
    string Name);