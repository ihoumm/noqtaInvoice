using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.CreateCustomer;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Domain.Common;

namespace NOQTA.Invoice.Application.Tests.Customers.CreateCustomer;

public sealed class CreateCustomerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesCustomerAndSaves()
    {
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCustomerService(
            customerRepository,
            unitOfWork);

        var command = new CreateCustomerCommand(
            "company",
            "ACME SARL",
            "contact@acme.ma",
            "10 Avenue Mohammed V",
            "Casablanca",
            "20000",
            "Morocco");

        var result = await service.ExecuteAsync(command);

        Assert.NotEqual(Guid.Empty, result.CustomerId);
        Assert.Equal("ACME SARL", result.Name);

        var customer = Assert.Single(customerRepository.AddedCustomers);

        Assert.Equal(result.CustomerId, customer.Id);
        Assert.Equal(CustomerType.Company, customer.Type);
        Assert.Equal("ACME SARL", customer.Name);
        Assert.Equal("contact@acme.ma", customer.Email.ToString());
        Assert.Equal(
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"),
            customer.Address);
        Assert.True(customer.IsActive);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutAddress_CreatesCustomerWithoutAddress()
    {
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCustomerService(
            customerRepository,
            unitOfWork);

        var command = new CreateCustomerCommand(
            "individual",
            "John Doe",
            "john@example.com",
            null,
            null,
            null,
            null);

        var result = await service.ExecuteAsync(command);

        var customer = Assert.Single(customerRepository.AddedCustomers);

        Assert.Equal(result.CustomerId, customer.Id);
        Assert.Null(customer.Address);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameIsInvalid_Throws()
    {
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCustomerService(
            customerRepository,
            unitOfWork);

        var command = new CreateCustomerCommand(
            "company",
            " ",
            "contact@acme.ma",
            null,
            null,
            null,
            null);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Empty(customerRepository.AddedCustomers);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsInvalid_Throws()
    {
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCustomerService(
            customerRepository,
            unitOfWork);

        var command = new CreateCustomerCommand(
            "company",
            "ACME SARL",
            "invalid-email",
            null,
            null,
            null,
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(command));

        Assert.Empty(customerRepository.AddedCustomers);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> AddedCustomers { get; } = new();

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task AddAsync(
            Customer customer,
            CancellationToken cancellationToken = default)
        {
            AddedCustomers.Add(customer);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Customer>>(
                AddedCustomers);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_InvalidCustomerType_ThrowsArgumentException()
    {
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new CreateCustomerService(
            customerRepository,
            unitOfWork);

        var command = new CreateCustomerCommand(
            "InvalidType",
            "ACME SARL",
            "contact@acme.ma",
            null,
            null,
            null,
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(command));
    }
}