using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.CancelInvoice;

public sealed class CancelInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelInvoiceService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        CancelInvoiceCommand command,
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

        invoice.Cancel();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}