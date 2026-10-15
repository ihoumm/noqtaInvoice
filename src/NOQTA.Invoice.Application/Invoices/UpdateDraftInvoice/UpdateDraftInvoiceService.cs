using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Invoices.UpdateDraftInvoice;

public sealed class UpdateDraftInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDraftInvoiceService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        UpdateDraftInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice =
            await _invoiceRepository.GetByIdAsync(
                command.InvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            throw new InvalidOperationException(
                $"Invoice '{command.InvoiceId}' was not found.");
        }

        foreach (var item in command.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                throw new InvalidOperationException(
                    "Item description is required.");
            }

            if (item.Quantity <= 0m)
            {
                throw new InvalidOperationException(
                    $"Quantity for '{item.Description}' must be greater than zero.");
            }

            if (item.UnitPrice < 0m)
            {
                throw new InvalidOperationException(
                    $"Unit price for '{item.Description}' cannot be negative.");
            }
        }

        if (command.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one invoice item is required.");
        }

        if (command.TaxRate < 0m)
        {
            throw new InvalidOperationException(
                "Tax rate cannot be negative.");
        }

        if (command.DiscountAmount < 0m)
        {
            throw new InvalidOperationException(
                "Discount amount cannot be negative.");
        }

        foreach (var existingItem in invoice.Items.ToList())
        {
            invoice.RemoveItem(existingItem.Id);
        }

        foreach (var item in command.Items)
        {
            invoice.AddItem(
                item.Description,
                item.Quantity,
                Money.Create(
                    item.UnitPrice,
                    invoice.Currency));
        }

        invoice.SetTaxRate(command.TaxRate);

        invoice.SetDiscount(
            Money.Create(
                command.DiscountAmount,
                invoice.Currency));

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}