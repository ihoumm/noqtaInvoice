using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.Company;

namespace NOQTA.Invoice.Infrastructure.Persistence.Repositories;

public sealed class CompanyProfileRepository : ICompanyProfileRepository
{
    private readonly InvoiceDbContext _dbContext;

    public CompanyProfileRepository(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyProfile?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CompanyProfile>()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(
        CompanyProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var existing = await _dbContext.Set<CompanyProfile>()
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            await _dbContext.Set<CompanyProfile>()
                .AddAsync(profile, cancellationToken);

            return;
        }

        if (existing.Id != profile.Id)
        {
            throw new InvalidOperationException(
                "A different company profile already exists.");
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(profile);

        var existingAddressEntry =
    _dbContext.Entry(existing)
        .Reference(x => x.Address)
        .TargetEntry;

        if (existingAddressEntry is null)
        {
            throw new InvalidOperationException(
                "Company profile address is not loaded.");
        }

        existingAddressEntry.CurrentValues.SetValues(
            profile.Address);
    }
}