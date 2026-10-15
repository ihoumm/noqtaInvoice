using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Company.GetCompanyProfile;

public sealed class GetCompanyProfileService
{
    private readonly ICompanyProfileRepository _repository;

    public GetCompanyProfileService(
        ICompanyProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanyProfileResult?> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var profile =
            await _repository.GetAsync(cancellationToken);

        if (profile is null)
            return null;

        return new CompanyProfileResult(
            profile.Id,
            profile.LegalName,
            profile.Email.Value,
            profile.Phone,
            profile.Address.Street,
            profile.Address.City,
            profile.Address.PostalCode,
            profile.Address.Country,
            profile.TaxIdentifier,
            profile.LogoPath,
            profile.CreatedAt);
    }
}