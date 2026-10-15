using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsPaid;
using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Tests.Invoices.MarkInvoiceAsPaid;

public sealed class MarkInvoiceAsPaidServiceTests
{
    [Fact]
    public async Task ExecuteAsync_SentInvoice_MarksAsPaidAndSaves()
    {
        var invoice = CreateSentInvoice();

        var repository = new FakeInvoiceRepository(invoice);
        var unitOfWork = new FakeUnitOfWork();

        var service = new MarkInvoiceAsPaidService(
            repository,
            unitOfWork);

        await service.ExecuteAsync(
            new MarkInvoiceAsPaidCommand(invoice.Id));

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);

        Assert.NotNull(invoice.PaidAt);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_MissingInvoice_ThrowsAndDoesNotSave()
    {
        var repository = new FakeInvoiceRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new MarkInvoiceAsPaidService(
            repository,
            unitOfWork);

        var invoiceId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                new MarkInvoiceAsPaidCommand(invoiceId)));

        Assert.Equal(
            $"Invoice '{invoiceId}' was not found.",
            exception.Message);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_DraftInvoice_ThrowsAndDoesNotSave()
    {
        var invoice = CreateDraftInvoice();

        var repository = new FakeInvoiceRepository(invoice);
        var unitOfWork = new FakeUnitOfWork();

        var service = new MarkInvoiceAsPaidService(
            repository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(
                new MarkInvoiceAsPaidCommand(invoice.Id)));

        Assert.Equal(
            InvoiceStatus.Draft,
            invoice.Status);

        Assert.Null(invoice.PaidAt);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyPaidInvoice_ThrowsAndDoesNotSave()
    {
        var invoice = CreateSentInvoice();

        invoice.MarkAsPaid();

        var paidAt = invoice.PaidAt;

        var repository = new FakeInvoiceRepository(invoice);
        var unitOfWork = new FakeUnitOfWork();

        var service = new MarkInvoiceAsPaidService(
            repository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(
                new MarkInvoiceAsPaidCommand(invoice.Id)));

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);

        Assert.Equal(
            paidAt,
            invoice.PaidAt);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    private static InvoiceAggregate CreateDraftInvoice()
    {
        return InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-TEST-PAID-001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "1 Rue Test",
                "Casablanca",
                "20000",
                "Morocco"));
    }

    private static InvoiceAggregate CreateSentInvoice()
    {
        var invoice = CreateDraftInvoice();

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(
                1500m,
                Currency.MAD));

        invoice.Issue();
        invoice.MarkAsSent();

        return invoice;
    }

    private sealed class FakeInvoiceRepository : IInvoiceRepository
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
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
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