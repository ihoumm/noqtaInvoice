using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.UpdateDraftInvoice;
using NOQTA.Invoice.Domain.Common;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Invoices.UpdateDraftInvoice;

public sealed class UpdateDraftInvoiceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesDraftInvoiceItemsTaxAndDiscount()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma");

        invoice.AddItem(
            "Old Service",
            1,
            Money.Create(500m, Currency.MAD));

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var command =
            new UpdateDraftInvoiceCommand(
                invoice.Id,
                new[]
                {
                    new UpdateDraftInvoiceItem(
                        "Brand Identity",
                        2,
                        1500m),
                    new UpdateDraftInvoiceItem(
                        "Website Design",
                        1,
                        2500m)
                },
                20m,
                500m);

        await service.ExecuteAsync(command);

        Assert.Equal(2, invoice.Items.Count);

        var items =
            invoice.Items
                .OrderBy(item => item.Description)
                .ToList();

        Assert.Equal("Brand Identity", items[0].Description);
        Assert.Equal(2m, items[0].Quantity);
        Assert.Equal(1500m, items[0].UnitPrice.Amount);

        Assert.Equal("Website Design", items[1].Description);
        Assert.Equal(1m, items[1].Quantity);
        Assert.Equal(2500m, items[1].UnitPrice.Amount);

        Assert.Equal(20m, invoice.TaxRate);
        Assert.Equal(500m, invoice.DiscountAmount.Amount);
        Assert.Equal(Currency.MAD, invoice.DiscountAmount.Currency);

        Assert.Equal(5500m, invoice.Totals.Subtotal.Amount);
        Assert.Equal(500m, invoice.Totals.DiscountAmount.Amount);
        Assert.Equal(1000m, invoice.Totals.TaxAmount.Amount);
        Assert.Equal(6000m, invoice.Totals.Total.Amount);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceDoesNotExist_Throws()
    {
        var invoiceRepository =
            new FakeInvoiceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var invoiceId =
            Guid.NewGuid();

        var command =
            new UpdateDraftInvoiceCommand(
                invoiceId,
                new[]
                {
                    new UpdateDraftInvoiceItem(
                        "Brand Identity",
                        1,
                        1500m)
                },
                0m,
                0m);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ExecuteAsync(command));

        Assert.Contains(
            invoiceId.ToString(),
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemsAreEmpty_Throws()
    {
        var invoice =
            CreateDraftInvoice();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var command =
            new UpdateDraftInvoiceCommand(
                invoice.Id,
                Array.Empty<UpdateDraftInvoiceItem>(),
                0m,
                0m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(command));

        Assert.Single(invoice.Items);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsInvalid_ThrowsWithoutChangingInvoice()
    {
        var invoice =
            CreateDraftInvoice();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var command =
            new UpdateDraftInvoiceCommand(
                invoice.Id,
                new[]
                {
                    new UpdateDraftInvoiceItem(
                        "Brand Identity",
                        0m,
                        1500m)
                },
                0m,
                0m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(command));

        var item =
            Assert.Single(invoice.Items);

        Assert.Equal("Original Service", item.Description);
        Assert.Equal(1m, item.Quantity);
        Assert.Equal(500m, item.UnitPrice.Amount);

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDiscountExceedsSubtotal_ThrowsWithoutSaving()
    {
        var invoice =
            CreateDraftInvoice();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var command =
            new UpdateDraftInvoiceCommand(
                invoice.Id,
                new[]
                {
                    new UpdateDraftInvoiceItem(
                        "Brand Identity",
                        1m,
                        1500m)
                },
                0m,
                2000m);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Single(invoice.Items);
        Assert.Equal(
            "Brand Identity",
            invoice.Items.Single().Description);

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsNotDraft_ThrowsWithoutSaving()
    {
        var invoice =
            CreateDraftInvoice();

        invoice.Issue();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new UpdateDraftInvoiceService(
                invoiceRepository,
                unitOfWork);

        var command =
            new UpdateDraftInvoiceCommand(
                invoice.Id,
                new[]
                {
                    new UpdateDraftInvoiceItem(
                        "Updated Service",
                        1m,
                        2000m)
                },
                0m,
                0m);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            InvoiceStatus.Issued,
            invoice.Status);

        Assert.Equal(
            "Original Service",
            invoice.Items.Single().Description);

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    private static InvoiceAggregate CreateDraftInvoice()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma");

        invoice.AddItem(
            "Original Service",
            1,
            Money.Create(500m, Currency.MAD));

        return invoice;
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