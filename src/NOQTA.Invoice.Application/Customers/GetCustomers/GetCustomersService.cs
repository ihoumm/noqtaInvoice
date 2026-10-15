using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Customers.GetCustomers;

public sealed class GetCustomersService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersService(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<CustomerListItem>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(
            cancellationToken);

        return customers
            .Select(customer => new CustomerListItem(
                customer.Id,
                customer.Type,
                customer.Name,
                customer.Email.ToString(),
                customer.IsActive))
            .ToList();
    }
}