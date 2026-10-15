using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.GetCustomers;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Customers.GetCustomers;

public sealed class GetCustomersServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsCustomerListItems()
    {
        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"));

        var customerRepository =
            new FakeCustomerRepository(customer);

        var service = new GetCustomersService(
            customerRepository);

        var result = await service.ExecuteAsync();

        var item = Assert.Single(result);

        Assert.Equal(customer.Id, item.Id);
        Assert.Equal(CustomerType.Company, item.Type);
        Assert.Equal("ACME SARL", item.Name);
        Assert.Equal("contact@acme.ma", item.Email);
        Assert.True(item.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryIsEmpty_ReturnsEmptyList()
    {
        var customerRepository =
            new FakeCustomerRepository();

        var service = new GetCustomersService(
            customerRepository);

        var result = await service.ExecuteAsync();

        Assert.Empty(result);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly IReadOnlyList<Customer> _customers;

        public FakeCustomerRepository(params Customer[] customers)
        {
            _customers = customers;
        }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _customers.FirstOrDefault(
                    customer => customer.Id == id));
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_customers);
        }
    }
}