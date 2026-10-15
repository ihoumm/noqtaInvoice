using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _dbContext;

    public InvoiceRepository(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InvoiceAggregate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<InvoiceAggregate>()
            .Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(
                invoice => invoice.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        InvoiceAggregate invoice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        await _dbContext.Set<InvoiceAggregate>()
            .AddAsync(invoice, cancellationToken);
    }

    public async Task<IReadOnlyList<InvoiceAggregate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var invoices = await _dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .ToListAsync(cancellationToken);

        return invoices
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ToList();
    }
}