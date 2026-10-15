using NOQTA.Invoice.Application.Abstractions.Files;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.Pdf;

namespace NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;

public sealed class GenerateInvoicePdfService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceDocumentBuilder _documentBuilder;
    private readonly IInvoicePdfRenderer _pdfRenderer;
    private readonly IFileStorage _fileStorage;

    public GenerateInvoicePdfService(
        IInvoiceRepository invoiceRepository,
        IInvoiceDocumentBuilder documentBuilder,
        IInvoicePdfRenderer pdfRenderer,
        IFileStorage fileStorage)
    {
        _invoiceRepository = invoiceRepository;
        _documentBuilder = documentBuilder;
        _pdfRenderer = pdfRenderer;
        _fileStorage = fileStorage;
    }

    public async Task<GenerateInvoicePdfResult?> ExecuteAsync(
        GenerateInvoicePdfCommand command,
        CancellationToken cancellationToken = default)
    {
        var invoice =
            await _invoiceRepository.GetByIdAsync(
                command.InvoiceId,
                cancellationToken);

        if (invoice is null)
            return null;

        var document =
            await _documentBuilder.BuildAsync(
                invoice,
                cancellationToken);

        var pdf =
            await _pdfRenderer.RenderAsync(
                document,
                cancellationToken);

        var fileName =
            $"Invoice-{invoice.InvoiceNumber.Value}.pdf";

        var filePath =
            await _fileStorage.SaveAsync(
                fileName,
                pdf,
                cancellationToken);

        return new GenerateInvoicePdfResult(
            invoice.Id,
            invoice.InvoiceNumber.Value,
            filePath);
    }
}