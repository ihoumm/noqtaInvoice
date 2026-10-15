using NOQTA.Invoice.Domain.Customers;

namespace NOQTA.Invoice.Application.Customers.GetCustomers;

public sealed record CustomerListItem(
    Guid Id,
    CustomerType Type,
    string Name,
    string Email,
    bool IsActive);