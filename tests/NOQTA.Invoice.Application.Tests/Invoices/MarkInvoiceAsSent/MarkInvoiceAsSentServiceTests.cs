using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsSent;
using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.ValueObjects;

using NOQTA.Invoice.Domain.Invoices;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Tests.Invoices.MarkInvoiceAsSent;

public sealed class MarkInvoiceAsSentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_MarksIssuedInvoiceAsSent()
    {
        var invoice = CreateIssuedInvoice();

        var repository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarkInvoiceAsSentService(
                repository,
                unitOfWork);

        await service.ExecuteAsync(
            new MarkInvoiceAsSentCommand(invoice.Id));

        Assert.Equal(
            InvoiceStatus.Sent,
            invoice.Status);

        Assert.NotNull(invoice.SentAt);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceDoesNotExist_Throws()
    {
        var invoiceId = Guid.NewGuid();

        var repository =
            new FakeInvoiceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarkInvoiceAsSentService(
                repository,
                unitOfWork);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ExecuteAsync(
                    new MarkInvoiceAsSentCommand(invoiceId)));

        Assert.Contains(
            invoiceId.ToString(),
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsDraft_ThrowsDomainException()
    {
        var invoice = CreateDraftInvoiceWithItem();

        var repository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarkInvoiceAsSentService(
                repository,
                unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(
                new MarkInvoiceAsSentCommand(invoice.Id)));

        Assert.Equal(
            InvoiceStatus.Draft,
            invoice.Status);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsAlreadySent_ThrowsDomainException()
    {
        var invoice = CreateIssuedInvoice();

        invoice.MarkAsSent();

        var repository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service =
            new MarkInvoiceAsSentService(
                repository,
                unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(
                new MarkInvoiceAsSentCommand(invoice.Id)));

        Assert.Equal(
            InvoiceStatus.Sent,
            invoice.Status);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    private static InvoiceAggregate CreateDraftInvoiceWithItem()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-TEST-000001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma",
                new Address(
                    "1 Rue Test",
                    "Casablanca",
                    "20000",
                    "Morocco"));

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(
                1500m,
                Currency.MAD));

        return invoice;
    }

    private static InvoiceAggregate CreateIssuedInvoice()
    {
        var invoice = CreateDraftInvoiceWithItem();

        invoice.Issue();

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
            if (_invoice is not null &&
                _invoice.Id == id)
            {
                return Task.FromResult<InvoiceAggregate?>(
                    _invoice);
            }

            return Task.FromResult<InvoiceAggregate?>(
                null);
        }

        public Task AddAsync(
            InvoiceAggregate invoice,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<InvoiceAggregate> invoices =
                _invoice is null
                    ? Array.Empty<InvoiceAggregate>()
                    : new[] { _invoice };

            return Task.FromResult(invoices);
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