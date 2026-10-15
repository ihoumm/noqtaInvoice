using NOQTA.Invoice.Domain.Customers;

namespace NOQTA.Invoice.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    CustomerType Type,
    string Name,
    string Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);