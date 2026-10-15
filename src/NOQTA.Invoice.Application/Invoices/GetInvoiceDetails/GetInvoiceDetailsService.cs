using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;

public sealed class GetInvoiceDetailsService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceDetailsService(
        IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDetailResult?> ExecuteAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice =
            await _invoiceRepository.GetByIdAsync(
                invoiceId,
                cancellationToken);

        if (invoice is null)
            return null;

        var totals = invoice.Totals;

        var items = invoice.Items
            .Select(item => new InvoiceDetailItem(
                item.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice.Amount,
                item.LineTotal.Amount,
                item.UnitPrice.Currency.Code))
            .ToList();

        InvoiceDetailAddress? address = null;

        if (invoice.CustomerAddress is not null)
        {
            address = new InvoiceDetailAddress(
                invoice.CustomerAddress.Street,
                invoice.CustomerAddress.City,
                invoice.CustomerAddress.PostalCode,
                invoice.CustomerAddress.Country);
        }

        return new InvoiceDetailResult(
            invoice.Id,
            invoice.InvoiceNumber.Value,
            invoice.Type.ToString(),
            invoice.Status.ToString(),
            invoice.Currency.Code,
            invoice.CustomerName,
            invoice.CustomerEmail.ToString(),
            address,
            invoice.TaxRate,
            totals.Subtotal.Amount,
            totals.DiscountAmount.Amount,
            totals.TaxAmount.Amount,
            totals.Total.Amount,
            invoice.CreatedAt,
            invoice.IssuedAt,
            invoice.SentAt,
            invoice.PaidAt,
            items);
    }
}