using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.GetInvoices;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Invoices.GetInvoices;

public sealed class GetInvoicesServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsInvoicesMappedToListItems()
    {
        var customerAddress = new Address(
            "123 Boulevard Mohammed V",
            "Casablanca",
            "20000",
            "Morocco");

        var invoice1 = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            customerAddress);

        invoice1.AddItem(
            "Brand Identity",
            2m,
            Money.Create(1500m, Currency.MAD));

        var invoice2 = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0002"),
            InvoiceType.Quote,
            Currency.MAD,
            "eDigiZoneMA",
            "contact@edizigone.ma");

        invoice2.AddItem(
            "Website Design",
            1m,
            Money.Create(759m, Currency.MAD));

        var repository =
            new FakeInvoiceRepository(
                invoice1,
                invoice2);

        var service =
            new GetInvoicesService(repository);

        var result =
            await service.ExecuteAsync();

        Assert.Equal(2, result.Count);

        var first = result[0];

        Assert.Equal(
            invoice1.Id,
            first.Id);

        Assert.Equal(
            "INV-2026-0001",
            first.InvoiceNumber);

        Assert.Equal(
            "Invoice",
            first.InvoiceType);

        Assert.Equal(
            "Draft",
            first.Status);

        Assert.Equal(
            "MAD",
            first.Currency);

        Assert.Equal(
            "ACME SARL",
            first.CustomerName);

        Assert.Equal(
            3000m,
            first.Total);

        Assert.Equal(
            invoice1.CreatedAt,
            first.CreatedAt);

        var second = result[1];

        Assert.Equal(
            "INV-2026-0002",
            second.InvoiceNumber);

        Assert.Equal(
            "Quote",
            second.InvoiceType);

        Assert.Equal(
            "eDigiZoneMA",
            second.CustomerName);

        Assert.Equal(
            759m,
            second.Total);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenRepositoryIsEmpty()
    {
        var repository =
            new FakeInvoiceRepository();

        var service =
            new GetInvoicesService(repository);

        var result =
            await service.ExecuteAsync();

        Assert.Empty(result);
    }

    private sealed class FakeInvoiceRepository
        : IInvoiceRepository
    {
        private readonly IReadOnlyList<InvoiceAggregate> _invoices;

        public FakeInvoiceRepository(
            params InvoiceAggregate[] invoices)
        {
            _invoices = invoices;
        }

        public Task<InvoiceAggregate?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var invoice =
                _invoices.FirstOrDefault(
                    x => x.Id == id);

            return Task.FromResult(invoice);
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
            return Task.FromResult(_invoices);
        }
    }
}