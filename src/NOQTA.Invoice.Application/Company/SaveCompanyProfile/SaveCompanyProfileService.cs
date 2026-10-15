using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Company.SaveCompanyProfile;

public sealed class SaveCompanyProfileService
{
    private readonly ICompanyProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveCompanyProfileService(
        ICompanyProfileRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        SaveCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing =
            await _repository.GetAsync(cancellationToken);

        var address = new Address(
            command.Street,
            command.City,
            command.PostalCode,
            command.Country);

        if (existing is null)
        {
            var profile = CompanyProfile.Create(
                command.LegalName,
                command.Email,
                command.Phone,
                address,
                command.TaxIdentifier,
                command.LogoPath);

            await _repository.SaveAsync(
                profile,
                cancellationToken);
        }
        else
        {
            existing.Update(
                command.LegalName,
                command.Email,
                command.Phone,
                address,
                command.TaxIdentifier,
                command.LogoPath);

            await _repository.SaveAsync(
                existing,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}