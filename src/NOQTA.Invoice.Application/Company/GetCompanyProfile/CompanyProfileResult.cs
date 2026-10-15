namespace NOQTA.Invoice.Application.Company.GetCompanyProfile;

public sealed record CompanyProfileResult(
    Guid Id,
    string LegalName,
    string Email,
    string Phone,
    string Street,
    string City,
    string? PostalCode,
    string Country,
    string? TaxIdentifier,
    string? LogoPath,
    DateTimeOffset CreatedAt);