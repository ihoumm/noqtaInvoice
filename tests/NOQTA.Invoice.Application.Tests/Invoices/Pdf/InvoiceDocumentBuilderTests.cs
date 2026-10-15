using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Application.Tests.Invoices.Pdf;

public sealed class InvoiceDocumentBuilderTests
{
    [Fact]
    public async Task BuildAsync_MapsCompanyInvoiceCustomerItemsAndTotals()
    {
        var companyProfile = CompanyProfile.Create(
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

        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "20 Boulevard Hassan II",
                "Casablanca",
                "20100",
                "Morocco"));

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(1500m, Currency.MAD));

        invoice.AddItem(
            "Website Design",
            1m,
            Money.Create(750m, Currency.MAD));

        invoice.SetTaxRate(20m);
        invoice.SetDiscount(
            Money.Create(500m, Currency.MAD));

        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = companyProfile
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        var result =
            await builder.BuildAsync(invoice);

        Assert.Equal(
            "NOQTA Agency",
            result.Company.LegalName);

        Assert.Equal(
            "contact@noqta.ma",
            result.Company.Email);

        Assert.Equal(
            "+212600000000",
            result.Company.Phone);

        Assert.Equal(
            "10 Avenue Mohammed V",
            result.Company.Street);

        Assert.Equal(
            "Casablanca",
            result.Company.City);

        Assert.Equal(
            "20000",
            result.Company.PostalCode);

        Assert.Equal(
            "Morocco",
            result.Company.Country);

        Assert.Equal(
            "ICE-123456789",
            result.Company.TaxIdentifier);

        Assert.Equal(
            "logos/noqta.png",
            result.Company.LogoPath);

        Assert.Equal(
            "INV-2026-0001",
            result.Invoice.InvoiceNumber);

        Assert.Equal(
            "Invoice",
            result.Invoice.InvoiceType);

        Assert.Equal(
            "Draft",
            result.Invoice.Status);

        Assert.Equal(
            "MAD",
            result.Invoice.Currency);

        Assert.Equal(
            invoice.CreatedAt,
            result.Invoice.CreatedAt);

        Assert.Null(result.Invoice.IssuedAt);
        Assert.Null(result.Invoice.SentAt);
        Assert.Null(result.Invoice.PaidAt);

        Assert.Equal(
            "ACME SARL",
            result.Customer.Name);

        Assert.Equal(
            "contact@acme.ma",
            result.Customer.Email);

        Assert.Equal(
            "20 Boulevard Hassan II",
            result.Customer.Street);

        Assert.Equal(
            "Casablanca",
            result.Customer.City);

        Assert.Equal(
            "20100",
            result.Customer.PostalCode);

        Assert.Equal(
            "Morocco",
            result.Customer.Country);

        Assert.Equal(2, result.Items.Count);

        var firstItem = result.Items[0];

        Assert.Equal(
            "Brand Identity",
            firstItem.Description);

        Assert.Equal(2m, firstItem.Quantity);
        Assert.Equal(1500m, firstItem.UnitPrice);
        Assert.Equal(3000m, firstItem.LineTotal);
        Assert.Equal("MAD", firstItem.Currency);

        var secondItem = result.Items[1];

        Assert.Equal(
            "Website Design",
            secondItem.Description);

        Assert.Equal(1m, secondItem.Quantity);
        Assert.Equal(750m, secondItem.UnitPrice);
        Assert.Equal(750m, secondItem.LineTotal);
        Assert.Equal("MAD", secondItem.Currency);

        Assert.Equal(3750m, result.Totals.Subtotal);
        Assert.Equal(500m, result.Totals.DiscountAmount);
        Assert.Equal(20m, result.Totals.TaxRate);
        Assert.Equal(650m, result.Totals.TaxAmount);
        Assert.Equal(3900m, result.Totals.Total);
        Assert.Equal("MAD", result.Totals.Currency);
    }

    [Fact]
    public async Task BuildAsync_WhenInvoiceHasNoCustomerAddress_ReturnsNullAddressFields()
    {
        var companyProfile = CreateCompanyProfile();

        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0002"),
            InvoiceType.Quote,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma");

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(1000m, Currency.MAD));

        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = companyProfile
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        var result =
            await builder.BuildAsync(invoice);

        Assert.Equal(
            "ACME SARL",
            result.Customer.Name);

        Assert.Equal(
            "contact@acme.ma",
            result.Customer.Email);

        Assert.Null(result.Customer.Street);
        Assert.Null(result.Customer.City);
        Assert.Null(result.Customer.PostalCode);
        Assert.Null(result.Customer.Country);
    }

    [Fact]
    public async Task BuildAsync_UsesInvoiceCustomerSnapshot()
    {
        var companyProfile = CreateCompanyProfile();

        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0003"),
            InvoiceType.Invoice,
            Currency.MAD,
            "Original Customer Name",
            "original@example.com",
            new Address(
                "Original Street",
                "Agadir",
                "80000",
                "Morocco"));

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(500m, Currency.MAD));

        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = companyProfile
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        var result =
            await builder.BuildAsync(invoice);

        Assert.Equal(
            "Original Customer Name",
            result.Customer.Name);

        Assert.Equal(
            "original@example.com",
            result.Customer.Email);

        Assert.Equal(
            "Original Street",
            result.Customer.Street);

        Assert.Equal(
            "Agadir",
            result.Customer.City);

        Assert.Equal(
            "80000",
            result.Customer.PostalCode);

        Assert.Equal(
            "Morocco",
            result.Customer.Country);
    }

    [Fact]
    public async Task BuildAsync_MapsInvoiceLifecycleDates()
    {
        var companyProfile = CreateCompanyProfile();

        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0004"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma");

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(1000m, Currency.MAD));

        invoice.Issue();
        invoice.MarkAsSent();
        invoice.MarkAsPaid();

        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = companyProfile
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        var result =
            await builder.BuildAsync(invoice);

        Assert.Equal(
            "Paid",
            result.Invoice.Status);

        Assert.NotNull(result.Invoice.IssuedAt);
        Assert.NotNull(result.Invoice.SentAt);
        Assert.NotNull(result.Invoice.PaidAt);
    }

    [Fact]
    public async Task BuildAsync_WhenCompanyProfileDoesNotExist_ThrowsInvalidOperationException()
    {
        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0005"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma");

        invoice.AddItem(
            "Consulting",
            1m,
            Money.Create(1000m, Currency.MAD));

        var repository =
            new FakeCompanyProfileRepository();

        var builder =
            new InvoiceDocumentBuilder(repository);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => builder.BuildAsync(invoice));

        Assert.Equal(
            "Company profile was not found.",
            exception.Message);
    }

    [Fact]
    public async Task BuildAsync_WhenInvoiceHasNoItems_ReturnsEmptyItemsAndZeroTotals()
    {
        var companyProfile = CreateCompanyProfile();

        var invoice = InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0006"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma");

        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = companyProfile
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        var result =
            await builder.BuildAsync(invoice);

        Assert.Empty(result.Items);

        Assert.Equal(
            0m,
            result.Totals.Subtotal);

        Assert.Equal(
            0m,
            result.Totals.DiscountAmount);

        Assert.Equal(
            0m,
            result.Totals.TaxRate);

        Assert.Equal(
            0m,
            result.Totals.TaxAmount);

        Assert.Equal(
            0m,
            result.Totals.Total);

        Assert.Equal(
            "MAD",
            result.Totals.Currency);
    }

    [Fact]
    public async Task BuildAsync_WhenInvoiceIsNull_ThrowsArgumentNullException()
    {
        var repository =
            new FakeCompanyProfileRepository
            {
                Profile = CreateCompanyProfile()
            };

        var builder =
            new InvoiceDocumentBuilder(repository);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => builder.BuildAsync(null!));
    }

    private static CompanyProfile CreateCompanyProfile()
    {
        return CompanyProfile.Create(
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