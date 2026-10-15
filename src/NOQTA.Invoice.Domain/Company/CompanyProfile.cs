using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Company;

public sealed class CompanyProfile : Entity
{
    // EF Core materialization constructor.
    private CompanyProfile()
        : base()
    {
    }

    private CompanyProfile(
        Guid id,
        string legalName,
        string email,
        string phone,
        Address address,
        string? taxIdentifier,
        string? logoPath)
        : base(id)
    {
        var normalizedLegalName =
            NormalizeOptional(legalName);

        var normalizedLogoPath =
            NormalizeOptional(logoPath);

        RequireCompanyIdentity(
            normalizedLegalName,
            normalizedLogoPath);

        LegalName = normalizedLegalName ?? string.Empty;
        Email = EmailAddress.Create(email);
        Phone = RequireValue(phone, "Phone");
        Address = address ?? throw new ArgumentNullException(nameof(address));
        TaxIdentifier = NormalizeOptional(taxIdentifier);
        LogoPath = normalizedLogoPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string LegalName { get; private set; } = null!;

    public EmailAddress Email { get; private set; }

    public string Phone { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public string? TaxIdentifier { get; private set; }

    public string? LogoPath { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static CompanyProfile Create(
        string legalName,
        string email,
        string phone,
        Address address,
        string? taxIdentifier = null,
        string? logoPath = null)
    {
        return new CompanyProfile(
            Guid.NewGuid(),
            legalName,
            email,
            phone,
            address,
            taxIdentifier,
            logoPath);
    }

    public void Update(
        string legalName,
        string email,
        string phone,
        Address address,
        string? taxIdentifier,
        string? logoPath)
    {
        var normalizedLegalName =
            NormalizeOptional(legalName);

        var normalizedLogoPath =
            NormalizeOptional(logoPath);

        RequireCompanyIdentity(
            normalizedLegalName,
            normalizedLogoPath);

        LegalName = normalizedLegalName ?? string.Empty;
        Email = EmailAddress.Create(email);
        Phone = RequireValue(phone, "Phone");
        Address = address ?? throw new ArgumentNullException(nameof(address));
        TaxIdentifier = NormalizeOptional(taxIdentifier);
        LogoPath = normalizedLogoPath;
    }

    public void SetLogo(string logoPath)
    {
        LogoPath = RequireValue(logoPath, "Logo path");
    }

    public void RemoveLogo()
    {
        LogoPath = null;
    }

    private static void RequireCompanyIdentity(
        string? legalName,
        string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(legalName) &&
            string.IsNullOrWhiteSpace(logoPath))
        {
            throw new DomainException(
                "Company name or logo is required.");
        }
    }

    private static string RequireValue(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}