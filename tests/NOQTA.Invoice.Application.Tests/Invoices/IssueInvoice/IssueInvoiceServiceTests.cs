using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.IssueInvoice;
using NOQTA.Invoice.Domain.Common;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Invoices.IssueInvoice;

public sealed class IssueInvoiceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_IssuesDraftInvoice()
    {
        var invoice = CreateDraftInvoiceWithItem();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new IssueInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new IssueInvoiceCommand(invoice.Id);

        await service.ExecuteAsync(command);

        Assert.Equal(
            InvoiceStatus.Issued,
            invoice.Status);

        Assert.NotNull(invoice.IssuedAt);

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

        var service = new IssueInvoiceService(
            invoiceRepository,
            unitOfWork);

        var invoiceId = Guid.NewGuid();

        var command =
            new IssueInvoiceCommand(invoiceId);

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
    public async Task ExecuteAsync_WhenInvoiceHasNoItems_ThrowsDomainException()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma");

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new IssueInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new IssueInvoiceCommand(invoice.Id);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            InvoiceStatus.Draft,
            invoice.Status);

        Assert.Null(invoice.IssuedAt);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsAlreadyIssued_ThrowsDomainException()
    {
        var invoice =
            CreateDraftInvoiceWithItem();

        invoice.Issue();

        var invoiceRepository =
            new FakeInvoiceRepository(invoice);

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new IssueInvoiceService(
            invoiceRepository,
            unitOfWork);

        var command =
            new IssueInvoiceCommand(invoice.Id);

        await Assert.ThrowsAsync<DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Equal(
            InvoiceStatus.Issued,
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