using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Invoices.Pdf;

public interface IInvoiceDocumentBuilder
{
    Task<InvoiceDocumentData> BuildAsync(
        InvoiceAggregate invoice,
        CancellationToken cancellationToken = default);
}