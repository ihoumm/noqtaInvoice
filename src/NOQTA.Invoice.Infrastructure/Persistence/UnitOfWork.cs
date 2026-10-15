using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly InvoiceDbContext _dbContext;

    public UnitOfWork(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}