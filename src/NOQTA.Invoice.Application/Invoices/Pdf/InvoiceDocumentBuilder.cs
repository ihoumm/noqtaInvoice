using NOQTA.Invoice.Application.Abstractions.Persistence;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Invoices.Pdf;

public sealed class InvoiceDocumentBuilder : IInvoiceDocumentBuilder
{
    private readonly ICompanyProfileRepository _companyProfileRepository;

    public InvoiceDocumentBuilder(
        ICompanyProfileRepository companyProfileRepository)
    {
        _companyProfileRepository = companyProfileRepository;
    }

    public async Task<InvoiceDocumentData> BuildAsync(
        InvoiceAggregate invoice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var companyProfile =
            await _companyProfileRepository.GetAsync(
                cancellationToken);

        if (companyProfile is null)
        {
            throw new InvalidOperationException(
                "Company profile was not found.");
        }

        var companyAddress = companyProfile.Address;
        var customerAddress = invoice.CustomerAddress;
        var totals = invoice.Totals;

        var items = invoice.Items
            .Select(item =>
                new InvoiceDocumentItem(
                    item.Description,
                    item.Quantity,
                    item.UnitPrice.Amount,
                    item.LineTotal.Amount,
                    item.UnitPrice.Currency.Code))
            .ToList();

        return new InvoiceDocumentData(
            new CompanyDocumentData(
                companyProfile.LegalName,
                companyProfile.Email.ToString(),
                companyProfile.Phone,
                companyAddress.Street,
                companyAddress.City,
                companyAddress.PostalCode,
                companyAddress.Country,
                companyProfile.TaxIdentifier,
                companyProfile.LogoPath),

            new InvoiceDocumentInfo(
                invoice.InvoiceNumber.Value,
                invoice.Type.ToString(),
                invoice.Status.ToString(),
                invoice.Currency.Code,
                invoice.CreatedAt,
                invoice.IssuedAt,
                invoice.SentAt,
                invoice.PaidAt),

            new CustomerDocumentData(
                invoice.CustomerName,
                invoice.CustomerEmail.ToString(),
                customerAddress?.Street,
                customerAddress?.City,
                customerAddress?.PostalCode,
                customerAddress?.Country),

            items,

            new InvoiceDocumentTotals(
                totals.Subtotal.Amount,
                totals.DiscountAmount.Amount,
                invoice.TaxRate,
                totals.TaxAmount.Amount,
                totals.Total.Amount,
                invoice.Currency.Code));
    }
}