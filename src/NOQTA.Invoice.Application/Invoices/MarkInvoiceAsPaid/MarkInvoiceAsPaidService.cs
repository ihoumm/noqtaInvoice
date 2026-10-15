using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.MarkInvoiceAsPaid;

public sealed class MarkInvoiceAsPaidService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkInvoiceAsPaidService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        MarkInvoiceAsPaidCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice =
            await _invoiceRepository.GetByIdAsync(
                command.InvoiceId,
                cancellationToken);

        if (invoice is null)
            throw new InvalidOperationException(
                $"Invoice '{command.InvoiceId}' was not found.");

        invoice.MarkAsPaid();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}