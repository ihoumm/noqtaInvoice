using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.CreateInvoice;
using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.Invoices;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Invoices.CreateInvoice;

public sealed class CreateInvoiceServiceTests
{
[Fact]
public async Task ExecuteAsync_CreatesInvoiceFromCustomerSnapshot()
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

    var customerRepository = new FakeCustomerRepository(customer);
    var invoiceRepository = new FakeInvoiceRepository();
    var unitOfWork = new FakeUnitOfWork();
    var invoiceNumberGenerator = new FakeInvoiceNumberGenerator(1);

    var service = new CreateInvoiceService(
        customerRepository,
        invoiceNumberGenerator,
        invoiceRepository,
        unitOfWork);

    var command = new CreateInvoiceCommand(
        customer.Id,
        InvoiceType.Invoice.ToString(),
        "MAD",
        new[]
        {
            new CreateInvoiceItem("Brand Identity", 2, 1500m)
        });

    var result = await service.ExecuteAsync(command);

    var invoice = Assert.Single(invoiceRepository.AddedInvoices);

    Assert.NotEqual(Guid.Empty, result.InvoiceId);
    Assert.Equal("Invoice ACME SARL - 000001", result.InvoiceNumber);
    Assert.Equal(result.InvoiceNumber, invoice.InvoiceNumber.Value);
    Assert.Equal(1, invoiceNumberGenerator.Calls);

    Assert.Equal(customer.Name, invoice.CustomerName);
    Assert.Equal(customer.Email.ToString(), invoice.CustomerEmail.ToString());
    Assert.Equal(customer.Address, invoice.CustomerAddress);

    var item = Assert.Single(invoice.Items);

    Assert.Equal("Brand Identity", item.Description);
    Assert.Equal(2m, item.Quantity);
    Assert.Equal(1500m, item.UnitPrice.Amount);
    Assert.Equal(Currency.MAD, item.UnitPrice.Currency);

    Assert.Equal(3000m, invoice.Totals.Subtotal.Amount);
    Assert.Equal(3000m, invoice.Totals.Total.Amount);
    Assert.Equal(1, unitOfWork.SaveChangesCalls);
}

[Fact]
public async Task ExecuteAsync_WhenCustomerDoesNotExist_Throws()
{
    var customerRepository = new FakeCustomerRepository();
    var invoiceRepository = new FakeInvoiceRepository();
    var unitOfWork = new FakeUnitOfWork();
    var invoiceNumberGenerator = new FakeInvoiceNumberGenerator(1);
    var customerId = Guid.NewGuid();

    var service = new CreateInvoiceService(
        customerRepository,
        invoiceNumberGenerator,
        invoiceRepository,
        unitOfWork);

    var command = new CreateInvoiceCommand(
        customerId,
        InvoiceType.Invoice.ToString(),
        "MAD",
        new[]
        {
            new CreateInvoiceItem("Brand Identity", 2, 1500m)
        });

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.ExecuteAsync(command));

    Assert.Contains(customerId.ToString(), exception.Message);
    Assert.Equal(0, invoiceNumberGenerator.Calls);
    Assert.Empty(invoiceRepository.AddedInvoices);
    Assert.Equal(0, unitOfWork.SaveChangesCalls);
}

[Fact]
public async Task ExecuteAsync_WhenCustomerIsInactive_ThrowsDomainException()
{
    var customer = Customer.Create(
        CustomerType.Company,
        "ACME SARL",
        "contact@acme.ma");

    customer.Deactivate();

    var customerRepository = new FakeCustomerRepository(customer);
    var invoiceRepository = new FakeInvoiceRepository();
    var unitOfWork = new FakeUnitOfWork();
    var invoiceNumberGenerator = new FakeInvoiceNumberGenerator(1);

    var service = new CreateInvoiceService(
        customerRepository,
        invoiceNumberGenerator,
        invoiceRepository,
        unitOfWork);

    var command = new CreateInvoiceCommand(
        customer.Id,
        InvoiceType.Invoice.ToString(),
        "MAD",
        new[]
        {
            new CreateInvoiceItem("Brand Identity", 2, 1500m)
        });

    await Assert.ThrowsAsync<DomainException>(
        () => service.ExecuteAsync(command));

    Assert.Equal(0, invoiceNumberGenerator.Calls);
    Assert.Empty(invoiceRepository.AddedInvoices);
    Assert.Equal(0, unitOfWork.SaveChangesCalls);
}

[Fact]
public async Task ExecuteAsync_WhenInvoiceTypeIsInvalid_ThrowsArgumentException()
{
    var customer = Customer.Create(
        CustomerType.Company,
        "ACME SARL",
        "contact@acme.ma");

    var customerRepository = new FakeCustomerRepository(customer);
    var invoiceRepository = new FakeInvoiceRepository();
    var unitOfWork = new FakeUnitOfWork();
    var invoiceNumberGenerator = new FakeInvoiceNumberGenerator(1);

    var service = new CreateInvoiceService(
        customerRepository,
        invoiceNumberGenerator,
        invoiceRepository,
        unitOfWork);

    var command = new CreateInvoiceCommand(
        customer.Id,
        "NotAValidInvoiceType",
        "MAD",
        new[]
        {
            new CreateInvoiceItem("Brand Identity", 2, 1500m)
        });

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    Assert.Equal(0, invoiceNumberGenerator.Calls);
    Assert.Empty(invoiceRepository.AddedInvoices);
    Assert.Equal(0, unitOfWork.SaveChangesCalls);
}

[Fact]
public async Task ExecuteAsync_UsesGlobalSequenceValue()
{
    var customer = Customer.Create(
        CustomerType.Company,
        "ACME SARL",
        "contact@acme.ma");

    var customerRepository = new FakeCustomerRepository(customer);
    var invoiceRepository = new FakeInvoiceRepository();
    var invoiceNumberGenerator = new FakeInvoiceNumberGenerator(37);
    var unitOfWork = new FakeUnitOfWork();

    var service = new CreateInvoiceService(
        customerRepository,
        invoiceNumberGenerator,
        invoiceRepository,
        unitOfWork);

    var command = new CreateInvoiceCommand(
        customer.Id,
        InvoiceType.Invoice.ToString(),
        "MAD",
        new[]
        {
            new CreateInvoiceItem("Brand Identity", 1, 1500m)
        });

    var result = await service.ExecuteAsync(command);

    Assert.Equal("Invoice ACME SARL - 000037", result.InvoiceNumber);
    Assert.Equal(1, invoiceNumberGenerator.Calls);
}

private sealed class FakeInvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly long _nextNumber;

    public int Calls { get; private set; }

    public FakeInvoiceNumberGenerator(long nextNumber)
    {
        _nextNumber = nextNumber;
    }

    public Task<long> GetNextAsync(
        CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(_nextNumber);
    }
}

private sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly Customer? _customer;

    public FakeCustomerRepository(Customer? customer = null)
    {
        _customer = customer;
    }

    public Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            _customer?.Id == id ? _customer : null);
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

private sealed class FakeInvoiceRepository : IInvoiceRepository
{
    public List<InvoiceAggregate> AddedInvoices { get; } = new();

    public Task<InvoiceAggregate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<InvoiceAggregate?>(
            AddedInvoices.FirstOrDefault(invoice => invoice.Id == id));
    }

    public Task AddAsync(
        InvoiceAggregate invoice,
        CancellationToken cancellationToken = default)
    {
        AddedInvoices.Add(invoice);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<InvoiceAggregate>>(
            AddedInvoices);
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