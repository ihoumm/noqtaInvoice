using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.DeactivateCustomer;
using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;
using Xunit;

namespace NOQTA.Invoice.Application.Tests.Customers.DeactivateCustomer;

public sealed class DeactivateCustomerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ActiveCustomer_DeactivatesAndSaves()
    {
        var customer = Customer.Create(
    CustomerType.Company,
    "ACME SARL",
    "contact@acme.ma",
    new Address(
        "10 Rue Mohammed V",
        "Casablanca",
        "20000",
        "Morocco"));

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DeactivateCustomerService(
                repository,
                unitOfWork);

        var command =
            new DeactivateCustomerCommand(
                customer.Id);

        var result =
            await service.ExecuteAsync(command);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal("ACME SARL", result.Name);
        Assert.False(customer.IsActive);
        Assert.NotNull(customer.DeactivatedAt);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_MissingCustomer_ThrowsAndDoesNotSave()
    {
        var customerId =
            Guid.NewGuid();

        var repository =
            new FakeCustomerRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DeactivateCustomerService(
                repository,
                unitOfWork);

        var command =
            new DeactivateCustomerCommand(
                customerId);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ExecuteAsync(command));

        Assert.Equal(
            $"Customer '{customerId}' was not found.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_InactiveCustomer_ThrowsAndDoesNotSave()
    {
        var customer = Customer.Create(
    CustomerType.Company,
    "ACME SARL",
    "contact@acme.ma",
    new Address(
        "10 Rue Mohammed V",
        "Casablanca",
        "20000",
        "Morocco"));

        customer.Deactivate();

        var repository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new DeactivateCustomerService(
                repository,
                unitOfWork);

        var command =
            new DeactivateCustomerCommand(
                customer.Id);

        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ExecuteAsync(command));

        Assert.Equal(
            "Customer is already inactive.",
            exception.Message);

        Assert.False(customer.IsActive);
        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    private sealed class FakeCustomerRepository
        : ICustomerRepository
    {
        private readonly Customer? _customer;

        public FakeCustomerRepository(
            Customer? customer = null)
        {
            _customer = customer;
        }

        public Task<Customer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_customer is not null &&
                _customer.Id == id)
            {
                return Task.FromResult<Customer?>(
                    _customer);
            }

            return Task.FromResult<Customer?>(
                null);
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
            throw new NotSupportedException();
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
}