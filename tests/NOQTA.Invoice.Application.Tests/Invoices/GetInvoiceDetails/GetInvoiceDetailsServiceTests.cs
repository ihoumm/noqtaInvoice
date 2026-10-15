using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Tests.Invoices.GetInvoiceDetails;

public sealed class GetInvoiceDetailsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenInvoiceExists_MapsInvoiceDetails()
    {
        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"));

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(1500m, Currency.MAD));

        invoice.AddItem(
            "Website Design",
            1m,
            Money.Create(750m, Currency.MAD));

        var repository = new FakeInvoiceRepository(invoice);

        var service =
            new GetInvoiceDetailsService(repository);

        var result =
            await service.ExecuteAsync(invoice.Id);

        Assert.NotNull(result);

        Assert.Equal(invoice.Id, result.Id);
        Assert.Equal("INV-2026-0001", result.InvoiceNumber);
        Assert.Equal("Invoice", result.InvoiceType);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("MAD", result.Currency);

        Assert.Equal("ACME SARL", result.CustomerName);
        Assert.Equal(
            "contact@acme.ma",
            result.CustomerEmail);

        Assert.NotNull(result.CustomerAddress);
        Assert.Equal(
            "10 Avenue Mohammed V",
            result.CustomerAddress.Street);
        Assert.Equal(
            "Casablanca",
            result.CustomerAddress.City);
        Assert.Equal(
            "20000",
            result.CustomerAddress.PostalCode);
        Assert.Equal(
            "Morocco",
            result.CustomerAddress.Country);

        Assert.Equal(0m, result.TaxRate);
        Assert.Equal(3750m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(0m, result.TaxAmount);
        Assert.Equal(3750m, result.Total);

        Assert.Equal(2, result.Items.Count);

        var firstItem = result.Items[0];

        Assert.Equal("Brand Identity", firstItem.Description);
        Assert.Equal(2m, firstItem.Quantity);
        Assert.Equal(1500m, firstItem.UnitPrice);
        Assert.Equal(3000m, firstItem.LineTotal);
        Assert.Equal("MAD", firstItem.Currency);

        var secondItem = result.Items[1];

        Assert.Equal("Website Design", secondItem.Description);
        Assert.Equal(1m, secondItem.Quantity);
        Assert.Equal(750m, secondItem.UnitPrice);
        Assert.Equal(750m, secondItem.LineTotal);
        Assert.Equal("MAD", secondItem.Currency);

        Assert.Equal(invoice.CreatedAt, result.CreatedAt);
        Assert.Null(result.IssuedAt);
        Assert.Null(result.SentAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceDoesNotExist_ReturnsNull()
    {
        var repository =
            new FakeInvoiceRepository();

        var service =
            new GetInvoiceDetailsService(repository);

        var invoiceId = Guid.NewGuid();

        var result =
            await service.ExecuteAsync(invoiceId);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceHasNoAddress_ReturnsNullAddress()
    {
        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0002"),
            InvoiceType.Quote,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma");

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(1000m, Currency.MAD));

        var repository =
            new FakeInvoiceRepository(invoice);

        var service =
            new GetInvoiceDetailsService(repository);

        var result =
            await service.ExecuteAsync(invoice.Id);

        Assert.NotNull(result);
        Assert.Null(result.CustomerAddress);
        Assert.Equal("Quote", result.InvoiceType);
        Assert.Equal(1000m, result.Total);
    }

    private sealed class FakeInvoiceRepository
        : IInvoiceRepository
    {
        private readonly InvoiceAggregate? _invoice;

        public FakeInvoiceRepository(
            InvoiceAggregate? invoice = null)
        {
            _invoice = invoice;
        }

        public Task<InvoiceAggregate?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _invoice?.Id == id
                    ? _invoice
                    : null);
        }

        public Task AddAsync(
            InvoiceAggregate invoice,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}