using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Company.GetCompanyProfile;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Tests.Company.GetCompanyProfile;

public sealed class GetCompanyProfileServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenProfileDoesNotExist_ReturnsNull()
    {
        var repository = new FakeCompanyProfileRepository();

        var service = new GetCompanyProfileService(repository);

        var result = await service.ExecuteAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfileExists_ReturnsMappedProfile()
    {
        var profile = CompanyProfile.Create(
            "NOQTA Agency",
            "contact@noqta.ma",
            "+212600000000",
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"),
            "ICE-123456789",
            "logos/noqta.png");

        var repository = new FakeCompanyProfileRepository
        {
            Profile = profile
        };

        var service = new GetCompanyProfileService(repository);

        var result = await service.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal(profile.Id, result.Id);
        Assert.Equal("NOQTA Agency", result.LegalName);
        Assert.Equal("contact@noqta.ma", result.Email);
        Assert.Equal("+212600000000", result.Phone);
        Assert.Equal("10 Avenue Mohammed V", result.Street);
        Assert.Equal("Casablanca", result.City);
        Assert.Equal("20000", result.PostalCode);
        Assert.Equal("Morocco", result.Country);
        Assert.Equal("ICE-123456789", result.TaxIdentifier);
        Assert.Equal("logos/noqta.png", result.LogoPath);
        Assert.Equal(profile.CreatedAt, result.CreatedAt);
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
}