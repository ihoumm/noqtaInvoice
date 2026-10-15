using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Company.SaveCompanyProfile;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Company.SaveCompanyProfile;

public sealed class SaveCompanyProfileServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenProfileDoesNotExist_CreatesAndSavesProfile()
    {
        var repository = new FakeCompanyProfileRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var command = CreateCommand();

        await service.ExecuteAsync(command);

        Assert.NotNull(repository.Profile);
        Assert.Equal("NOQTA Agency", repository.Profile.LegalName);
        Assert.Equal("contact@noqta.ma", repository.Profile.Email.Value);
        Assert.Equal("+212600000000", repository.Profile.Phone);
        Assert.Equal(
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"),
            repository.Profile.Address);
        Assert.Equal("ICE-123456789", repository.Profile.TaxIdentifier);
        Assert.Equal("logos/noqta.png", repository.Profile.LogoPath);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfileExists_UpdatesExistingProfile()
    {
        var existingProfile = CompanyProfile.Create(
            "Old Company",
            "old@example.com",
            "+212600000000",
            new Address(
                "Old Street",
                "Rabat",
                "10000",
                "Morocco"),
            "OLD-123",
            "logos/old.png");

        var repository = new FakeCompanyProfileRepository
        {
            Profile = existingProfile
        };

        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var originalId = existingProfile.Id;
        var originalCreatedAt = existingProfile.CreatedAt;

        await service.ExecuteAsync(CreateCommand());

        Assert.NotNull(repository.Profile);
        Assert.Equal(originalId, repository.Profile.Id);
        Assert.Equal(originalCreatedAt, repository.Profile.CreatedAt);
        Assert.Equal("NOQTA Agency", repository.Profile.LegalName);
        Assert.Equal("contact@noqta.ma", repository.Profile.Email.Value);
        Assert.Equal("+212600000000", repository.Profile.Phone);
        Assert.Equal(
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"),
            repository.Profile.Address);
        Assert.Equal("ICE-123456789", repository.Profile.TaxIdentifier);
        Assert.Equal("logos/noqta.png", repository.Profile.LogoPath);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaxIdentifierAndLogoAreBlank_NormalizesThemToNull()
    {
        var repository = new FakeCompanyProfileRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var command = new SaveCompanyProfileCommand(
            "NOQTA Agency",
            "contact@noqta.ma",
            "+212600000000",
            "10 Avenue Mohammed V",
            "Casablanca",
            "20000",
            "Morocco",
            " ",
            " ");

        await service.ExecuteAsync(command);

        Assert.NotNull(repository.Profile);
        Assert.Null(repository.Profile.TaxIdentifier);
        Assert.Null(repository.Profile.LogoPath);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLegalNameIsBlankButLogoExists_SavesProfile()
    {
        var repository = new FakeCompanyProfileRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var command = CreateCommand() with
        {
            LegalName = " "
        };

        await service.ExecuteAsync(command);

        Assert.NotNull(repository.Profile);
        Assert.Equal(string.Empty, repository.Profile.LegalName);
        Assert.Equal("logos/noqta.png", repository.Profile.LogoPath);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLegalNameAndLogoAreBlank_DoesNotSave()
    {
        var repository = new FakeCompanyProfileRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var command = CreateCommand() with
        {
            LegalName = " ",
            LogoPath = " "
        };

        await Assert.ThrowsAsync<Domain.Common.DomainException>(
            () => service.ExecuteAsync(command));

        Assert.Null(repository.Profile);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsInvalid_DoesNotSave()
    {
        var repository = new FakeCompanyProfileRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new SaveCompanyProfileService(
            repository,
            unitOfWork);

        var command = CreateCommand() with
        {
            Email = "invalid-email"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(command));

        Assert.Null(repository.Profile);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    private static SaveCompanyProfileCommand CreateCommand()
    {
        return new SaveCompanyProfileCommand(
            "NOQTA Agency",
            "contact@noqta.ma",
            "+212600000000",
            "10 Avenue Mohammed V",
            "Casablanca",
            "20000",
            "Morocco",
            "ICE-123456789",
            "logos/noqta.png");
    }

    private sealed class FakeCompanyProfileRepository
        : ICompanyProfileRepository
    {
        public CompanyProfile? Profile { get; set; }

        public Task<CompanyProfile?> GetAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Profile);
        }

        public Task SaveAsync(
            CompanyProfile profile,
            CancellationToken cancellationToken = default)
        {
            Profile = profile;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}