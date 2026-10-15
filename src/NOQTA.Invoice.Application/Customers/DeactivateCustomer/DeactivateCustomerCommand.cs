namespace NOQTA.Invoice.Application.Customers.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(
    Guid CustomerId);