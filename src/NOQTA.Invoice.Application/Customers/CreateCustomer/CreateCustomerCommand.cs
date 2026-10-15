namespace NOQTA.Invoice.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string CustomerType,
    string Name,
    string Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);