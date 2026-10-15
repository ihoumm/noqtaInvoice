namespace NOQTA.Invoice.Application.Invoices.Pdf;

public interface IInvoicePdfRenderer
{
    Task<byte[]> RenderAsync(
        InvoiceDocumentData document,
        CancellationToken cancellationToken = default);
}