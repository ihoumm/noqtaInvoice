namespace NOQTA.Invoice.Application.Customers.ReactivateCustomer;

public sealed record ReactivateCustomerCommand(
    Guid CustomerId);