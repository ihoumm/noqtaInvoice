using NOQTA.Invoice.Application.Abstractions.Files;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Tests.Invoices.GenerateInvoicePdf;

public sealed class GenerateInvoicePdfServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GeneratesAndStoresPdf()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                Currency.MAD,
                "Atlas Studio",
                "billing@atlas-studio.ma");

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(1500m, Currency.MAD));

        var repository =
            new FakeInvoiceRepository(invoice);

        var documentBuilder =
            new FakeInvoiceDocumentBuilder();

        var renderer =
            new FakeInvoicePdfRenderer();

        var storage =
            new FakeFileStorage();

        var service =
            new GenerateInvoicePdfService(
                repository,
                documentBuilder,
                renderer,
                storage);

        var result =
            await service.ExecuteAsync(
                new GenerateInvoicePdfCommand(
                    invoice.Id));

        Assert.NotNull(result);
        Assert.Equal(
            invoice.Id,
            result!.InvoiceId);

        Assert.Equal(
            "INV-2026-0001",
            result.InvoiceNumber);

        Assert.Equal(
            "Invoice-INV-2026-0001.pdf",
            storage.SavedFileName);

        Assert.Equal(
            FakeInvoicePdfRenderer.PdfBytes,
            storage.SavedContent);

        Assert.NotNull(renderer.ReceivedDocument);

        Assert.Equal(
            "INV-2026-0001",
            renderer.ReceivedDocument!.Invoice.InvoiceNumber);

        Assert.Equal(
            "Invoice-INV-2026-0001.pdf",
            result.FilePath);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenInvoiceDoesNotExist()
    {
        var repository =
            new FakeInvoiceRepository(null);

        var documentBuilder =
            new FakeInvoiceDocumentBuilder();

        var renderer =
            new FakeInvoicePdfRenderer();

        var storage =
            new FakeFileStorage();

        var service =
            new GenerateInvoicePdfService(
                repository,
                documentBuilder,
                renderer,
                storage);

        var result =
            await service.ExecuteAsync(
                new GenerateInvoicePdfCommand(
                    Guid.NewGuid()));

        Assert.Null(result);
        Assert.False(
            documentBuilder.WasCalled);
        Assert.False(
            renderer.WasCalled);
        Assert.False(
            storage.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotModifyInvoice()
    {
        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create("INV-2026-0002"),
                InvoiceType.Invoice,
                Currency.MAD,
                "Atlas Studio",
                "billing@atlas-studio.ma");

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(1000m, Currency.MAD));

        var originalStatus =
            invoice.Status;

        var originalIssuedAt =
            invoice.IssuedAt;

        var originalSentAt =
            invoice.SentAt;

        var originalPaidAt =
            invoice.PaidAt;

        var repository =
            new FakeInvoiceRepository(invoice);

        var service =
            new GenerateInvoicePdfService(
                repository,
                new FakeInvoiceDocumentBuilder(),
                new FakeInvoicePdfRenderer(),
                new FakeFileStorage());

        await service.ExecuteAsync(
            new GenerateInvoicePdfCommand(
                invoice.Id));

        Assert.Equal(
            originalStatus,
            invoice.Status);

        Assert.Equal(
            originalIssuedAt,
            invoice.IssuedAt);

        Assert.Equal(
            originalSentAt,
            invoice.SentAt);

        Assert.Equal(
            originalPaidAt,
            invoice.PaidAt);
    }

    private sealed class FakeInvoiceRepository
        : IInvoiceRepository
    {
        private readonly InvoiceAggregate? _invoice;

        public FakeInvoiceRepository(
            InvoiceAggregate? invoice)
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

    private sealed class FakeInvoiceDocumentBuilder
        : IInvoiceDocumentBuilder
    {
        public static readonly InvoiceDocumentData Document =
            new(
                new CompanyDocumentData(
                    "NOQTA Agency",
                    "contact@noqta.ma",
                    "+212 600 000 000",
                    "Avenue Mohammed V",
                    "Agadir",
                    "80000",
                    "Morocco",
                    null,
                    null),
                new InvoiceDocumentInfo(
                    "INV-2026-0001",
                    "Invoice",
                    "Draft",
                    "MAD",
                    DateTimeOffset.UtcNow,
                    null,
                    null,
                    null),
                new CustomerDocumentData(
                    "Atlas Studio",
                    "billing@atlas-studio.ma",
                    null,
                    null,
                    null,
                    null),
                Array.Empty<InvoiceDocumentItem>(),
                new InvoiceDocumentTotals(
                    0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    "MAD"));

        public bool WasCalled { get; private set; }

        public Task<InvoiceDocumentData> BuildAsync(
            InvoiceAggregate invoice,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                Document with
                {
                    Invoice = Document.Invoice with
                    {
                        InvoiceNumber =
                            invoice.InvoiceNumber.Value
                    }
                });
        }
    }

    private sealed class FakeInvoicePdfRenderer
        : IInvoicePdfRenderer
    {
        public static readonly byte[] PdfBytes =
            new byte[] { 0x25, 0x50, 0x44, 0x46 };

        public bool WasCalled { get; private set; }

        public InvoiceDocumentData? ReceivedDocument { get; private set; }

        public Task<byte[]> RenderAsync(
            InvoiceDocumentData document,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedDocument = document;

            return Task.FromResult(PdfBytes);
        }
    }

    private sealed class FakeFileStorage
        : IFileStorage
    {
        public bool WasCalled { get; private set; }

        public string? SavedFileName { get; private set; }

        public byte[]? SavedContent { get; private set; }

        public Task<string> SaveAsync(
            string fileName,
            byte[] content,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            SavedFileName = fileName;
            SavedContent = content;

            return Task.FromResult(fileName);
        }
    }
}