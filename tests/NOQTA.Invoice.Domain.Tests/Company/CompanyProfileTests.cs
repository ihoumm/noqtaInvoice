using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Tests.Company;

public sealed class CompanyProfileTests
{
    [Fact]
    public void Create_CreatesValidCompanyProfile()
    {
        var profile = CreateProfile();

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("NOQTA Agency", profile.LegalName);
        Assert.Equal("contact@noqta.ma", profile.Email.Value);
        Assert.Equal("+212600000000", profile.Phone);
        Assert.Equal("ICE-123456789", profile.TaxIdentifier);
        Assert.Equal("logos/noqta.png", profile.LogoPath);
    }

    [Fact]
    public void Create_WithEmptyLegalName_Throws()
    {
        Assert.Throws<DomainException>(() =>
            CompanyProfile.Create(
                "",
                "contact@noqta.ma",
                "+212600000000",
                CreateAddress()));
    }

    [Fact]
    public void Create_WithEmptyLegalNameAndLogo_Succeeds()
    {
        var profile = CompanyProfile.Create(
            "",
            "contact@noqta.ma",
            "+212600000000",
            CreateAddress(),
            null,
            "logos/noqta.png");

        Assert.Equal(string.Empty, profile.LegalName);
        Assert.Equal("logos/noqta.png", profile.LogoPath);
    }

    [Fact]
    public void Update_WithEmptyLegalNameAndLogo_Succeeds()
    {
        var profile = CreateProfile();

        profile.Update(
            "",
            "hello@noqta.ma",
            "+212611111111",
            CreateAddress(),
            null,
            "logos/new-logo.png");

        Assert.Equal(string.Empty, profile.LegalName);
        Assert.Equal("logos/new-logo.png", profile.LogoPath);
    }

    [Fact]
    public void Update_WithEmptyLegalNameAndLogo_Throws()
    {
        var profile = CreateProfile();

        Assert.Throws<DomainException>(() =>
            profile.Update(
                "",
                "hello@noqta.ma",
                "+212611111111",
                CreateAddress(),
                null,
                ""));
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CompanyProfile.Create(
                "NOQTA Agency",
                "invalid-email",
                "+212600000000",
                CreateAddress()));
    }

    [Fact]
    public void Update_ReplacesCompanyInformation()
    {
        var profile = CreateProfile();

        var address = new Address(
            "20 Boulevard Zerktouni",
            "Casablanca",
            "20000",
            "Morocco");

        profile.Update(
            "NOQTA Agency SARL",
            "hello@noqta.ma",
            "+212611111111",
            address,
            "ICE-987654321",
            "logos/new-logo.png");

        Assert.Equal("NOQTA Agency SARL", profile.LegalName);
        Assert.Equal("hello@noqta.ma", profile.Email.Value);
        Assert.Equal("+212611111111", profile.Phone);
        Assert.Equal(address, profile.Address);
        Assert.Equal("ICE-987654321", profile.TaxIdentifier);
        Assert.Equal("logos/new-logo.png", profile.LogoPath);
    }

    [Fact]
    public void SetLogo_SetsLogoPath()
    {
        var profile = CreateProfile();

        profile.SetLogo("logos/updated.png");

        Assert.Equal("logos/updated.png", profile.LogoPath);
    }

    [Fact]
    public void RemoveLogo_ClearsLogoPath()
    {
        var profile = CreateProfile();

        profile.RemoveLogo();

        Assert.Null(profile.LogoPath);
    }

    [Fact]
    public void OptionalValues_AreNormalizedToNull()
    {
        var profile = CompanyProfile.Create(
            "NOQTA Agency",
            "contact@noqta.ma",
            "+212600000000",
            CreateAddress(),
            "   ",
            " ");

        Assert.Null(profile.TaxIdentifier);
        Assert.Null(profile.LogoPath);
    }

    private static CompanyProfile CreateProfile()
    {
        return CompanyProfile.Create(
            "NOQTA Agency",
            "contact@noqta.ma",
            "+212600000000",
            CreateAddress(),
            "ICE-123456789",
            "logos/noqta.png");
    }

    private static Address CreateAddress()
    {
        return new Address(
            "10 Avenue Mohammed V",
            "Casablanca",
            "20000",
            "Morocco");
    }
}