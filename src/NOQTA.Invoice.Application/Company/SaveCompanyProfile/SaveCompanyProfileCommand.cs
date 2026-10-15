namespace NOQTA.Invoice.Application.Company.SaveCompanyProfile;

public sealed record SaveCompanyProfileCommand(
    string LegalName,
    string Email,
    string Phone,
    string Street,
    string City,
    string PostalCode,
    string Country,
    string? TaxIdentifier,
    string? LogoPath);