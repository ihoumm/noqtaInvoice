using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.MarkInvoiceAsSent;

public sealed class MarkInvoiceAsSentService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkInvoiceAsSentService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        MarkInvoiceAsSentCommand command,
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

        invoice.MarkAsSent();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}