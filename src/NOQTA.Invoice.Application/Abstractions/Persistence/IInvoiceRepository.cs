using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task<InvoiceAggregate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        InvoiceAggregate invoice,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
        CancellationToken cancellationToken = default);
}