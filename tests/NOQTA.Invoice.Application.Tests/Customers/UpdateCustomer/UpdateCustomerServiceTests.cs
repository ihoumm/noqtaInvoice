using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.UpdateCustomer;
using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Customers.UpdateCustomer;

public sealed class UpdateCustomerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCustomerExists_UpdatesCustomerAndSaves()
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

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                customerRepository,
                unitOfWork);

        var command =
            new UpdateCustomerCommand(
                customer.Id,
                CustomerType.Entrepreneur,
                "Updated Agency",
                "hello@updated.ma",
                "25 Boulevard Zerktouni",
                "Marrakech",
                "40000",
                "Morocco");

        var result =
            await service.ExecuteAsync(command);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal("Updated Agency", result.Name);

        Assert.Equal(
            CustomerType.Entrepreneur,
            customer.Type);

        Assert.Equal(
            "Updated Agency",
            customer.Name);

        Assert.Equal(
            "hello@updated.ma",
            customer.Email.ToString());

        Assert.Equal(
            new Address(
                "25 Boulevard Zerktouni",
                "Marrakech",
                "40000",
                "Morocco"),
            customer.Address);

        Assert.True(customer.IsActive);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAddressIsOmitted_RemovesExistingAddress()
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

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                customerRepository,
                unitOfWork);

        var command =
            new UpdateCustomerCommand(
                customer.Id,
                CustomerType.Company,
                "ACME Updated",
                "updated@acme.ma",
                null,
                null,
                null,
                null);

        await service.ExecuteAsync(command);

        Assert.Equal(
            "ACME Updated",
            customer.Name);

        Assert.Equal(
            "updated@acme.ma",
            customer.Email.ToString());

        Assert.Null(customer.Address);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerDoesNotExist_Throws()
    {
        var customerRepository =
            new FakeCustomerRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                customerRepository,
                unitOfWork);

        var customerId =
            Guid.NewGuid();

        var command =
            new UpdateCustomerCommand(
                customerId,
                CustomerType.Company,
                "ACME SARL",
                "contact@acme.ma",
                null,
                null,
                null,
                null);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ExecuteAsync(command));

        Assert.Contains(
            customerId.ToString(),
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomerIsInactive_ThrowsDomainException()
    {
        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma");

        customer.Deactivate();

        var customerRepository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                customerRepository,
                unitOfWork);

        var command =
            new UpdateCustomerCommand(
                customer.Id,
                CustomerType.Company,
                "Updated Agency",
                "updated@acme.ma",
                null,
                null,
                null,
                null);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            "ACME SARL",
            customer.Name);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameIsInvalid_ThrowsDomainException()
    {
        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma");

        var customerRepository =
            new FakeCustomerRepository(customer);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateCustomerService(
                customerRepository,
                unitOfWork);

        var command =
            new UpdateCustomerCommand(
                customer.Id,
                CustomerType.Company,
                "   ",
                "updated@acme.ma",
                null,
                null,
                null,
                null);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            "ACME SARL",
            customer.Name);

        Assert.Equal(
            "contact@acme.ma",
            customer.Email.ToString());

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
public async Task ExecuteAsync_WhenEmailIsInvalid_ThrowsArgumentException()
{
    var customer = Customer.Create(
        CustomerType.Company,
        "ACME SARL",
        "contact@acme.ma");

    var customerRepository =
        new FakeCustomerRepository(customer);

    var unitOfWork =
        new FakeUnitOfWork();

    var service =
        new UpdateCustomerService(
            customerRepository,
            unitOfWork);

    var command =
        new UpdateCustomerCommand(
            customer.Id,
            CustomerType.Company,
            "Updated Agency",
            "not-an-email",
            null,
            null,
            null,
            null);

    var exception =
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(command));

    Assert.Equal(
        "Invalid email address. (Parameter 'value')",
        exception.Message);

    Assert.Equal(
        "Updated Agency",
        customer.Name);

    Assert.Equal(
        "contact@acme.ma",
        customer.Email.ToString());

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
            return Task.FromResult(
                _customer?.Id == id
                    ? _customer
                    : null);
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

    private sealed class FakeUnitOfWork
        : IUnitOfWork
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