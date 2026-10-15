using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Invoices.IssueInvoice;

public sealed class IssueInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueInvoiceService(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        IssueInvoiceCommand command,
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

        invoice.Issue();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}