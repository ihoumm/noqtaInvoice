using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.GetInvoices;

public sealed class GetInvoicesService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesService(
        IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IReadOnlyList<InvoiceListItem>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var invoices =
            await _invoiceRepository.GetAllAsync(
                cancellationToken);

        return invoices
            .Select(invoice => new InvoiceListItem(
                invoice.Id,
                invoice.InvoiceNumber.Value,
                invoice.Type.ToString(),
                invoice.Status.ToString(),
                invoice.Currency.Code,
                invoice.CustomerName,
                invoice.Totals.Total.Amount,
                invoice.CreatedAt))
            .ToList();
    }
}