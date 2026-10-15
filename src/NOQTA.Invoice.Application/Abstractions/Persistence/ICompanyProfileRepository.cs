using NOQTA.Invoice.Domain.Company;

namespace NOQTA.Invoice.Application.Abstractions.Persistence;

public interface ICompanyProfileRepository
{
    Task<CompanyProfile?> GetAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        CompanyProfile profile,
        CancellationToken cancellationToken = default);
}