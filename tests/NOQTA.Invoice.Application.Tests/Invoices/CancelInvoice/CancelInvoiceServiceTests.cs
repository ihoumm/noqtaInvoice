using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.CancelInvoice;
using NOQTA.Invoice.Domain.Common;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Invoices.CancelInvoice;

public sealed class CancelInvoiceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CancelsDraftInvoice()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new CancelInvoiceCommand(invoice.Id);

        await service.ExecuteAsync(command);

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_CancelsIssuedInvoice()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        invoice.Issue();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new CancelInvoiceCommand(invoice.Id);

        await service.ExecuteAsync(command);

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_CancelsSentInvoice()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        invoice.Issue();
        invoice.MarkAsSent();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new CancelInvoiceCommand(invoice.Id);

        await service.ExecuteAsync(command);

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceDoesNotExist_Throws()
    {
        var invoiceRepository =
            new FakeInvoiceRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var invoiceId = Guid.NewGuid();

        var command =
            new CancelInvoiceCommand(invoiceId);

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
    public async Task ExecuteAsync_WhenInvoiceIsPaid_ThrowsDomainException()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        invoice.Issue();
        invoice.MarkAsSent();
        invoice.MarkAsPaid();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new CancelInvoiceCommand(invoice.Id);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsAlreadyCancelled_ThrowsDomainException()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        invoice.Cancel();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new CancelInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new CancelInvoiceCommand(invoice.Id);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    private static InvoiceAggregate CreateDraftInvoiceWithItem()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma");

        invoice.AddItem(
            "Brand Identity",
            2,
            Money.Create(1500m, Currency.MAD));

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