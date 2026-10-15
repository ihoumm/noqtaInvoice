namespace NOQTA.Invoice.Application.Customers.DeactivateCustomer;

public sealed record DeactivateCustomerResult(
    Guid CustomerId,
    string Name);