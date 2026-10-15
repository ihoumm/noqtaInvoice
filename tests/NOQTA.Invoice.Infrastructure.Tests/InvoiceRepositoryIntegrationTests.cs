using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;

using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using CurrencyValue = NOQTA.Invoice.Domain.ValueObjects.Currency;
using MoneyValue = NOQTA.Invoice.Domain.ValueObjects.Money;

using InvoiceNumberValue = NOQTA.Invoice.Domain.Invoices.InvoiceNumber;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class InvoiceRepositoryIntegrationTests
{
    [Fact]
    public async Task GetByIdAsync_PersistedInvoice_ReloadsCompleteInvoiceGraph()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "10 Avenue Mohammed V",
                "Casablanca",
                "20000",
                "Morocco"));

        await dbContext.Customers.AddAsync(customer);

        var invoice = InvoiceAggregate.Create(
            InvoiceNumberValue.Create("INV-2026-0099"),
            InvoiceType.Invoice,
            CurrencyValue.MAD,
            customer.Name,
            customer.Email.ToString(),
            customer.Address);

        invoice.AddItem(
            "Brand Identity",
            2m,
            MoneyValue.Create(1500m, CurrencyValue.MAD));

        invoice.AddItem(
            "Website Design",
            1m,
            MoneyValue.Create(2500m, CurrencyValue.MAD));

        invoice.SetTaxRate(20m);
        invoice.SetDiscount(
            MoneyValue.Create(500m, CurrencyValue.MAD));

        await dbContext.Invoices.AddAsync(invoice);
        await dbContext.SaveChangesAsync();

        var repository = new InvoiceRepository(dbContext);

        var reloaded = await repository.GetByIdAsync(invoice.Id);

        Assert.NotNull(reloaded);

        Assert.Equal(invoice.Id, reloaded.Id);
        Assert.Equal("INV-2026-0099", reloaded.InvoiceNumber.Value);
        Assert.Equal(InvoiceType.Invoice, reloaded.Type);
        Assert.Equal(InvoiceStatus.Draft, reloaded.Status);
        Assert.Equal(CurrencyValue.MAD, reloaded.Currency);

        Assert.Equal("ACME SARL", reloaded.CustomerName);
        Assert.Equal(
            "contact@acme.ma",
            reloaded.CustomerEmail.ToString());

        Assert.NotNull(reloaded.CustomerAddress);
        Assert.Equal(
            "10 Avenue Mohammed V",
            reloaded.CustomerAddress.Street);
        Assert.Equal(
            "Casablanca",
            reloaded.CustomerAddress.City);
        Assert.Equal(
            "20000",
            reloaded.CustomerAddress.PostalCode);
        Assert.Equal(
            "Morocco",
            reloaded.CustomerAddress.Country);

        Assert.Equal(2, reloaded.Items.Count);

        var brandIdentity = reloaded.Items
            .Single(item => item.Description == "Brand Identity");

        Assert.Equal(2m, brandIdentity.Quantity);
        Assert.Equal(1500m, brandIdentity.UnitPrice.Amount);
        Assert.Equal(CurrencyValue.MAD, brandIdentity.UnitPrice.Currency);
        Assert.Equal(3000m, brandIdentity.LineTotal.Amount);

        var websiteDesign = reloaded.Items
            .Single(item => item.Description == "Website Design");

        Assert.Equal(1m, websiteDesign.Quantity);
        Assert.Equal(2500m, websiteDesign.UnitPrice.Amount);
        Assert.Equal(CurrencyValue.MAD, websiteDesign.UnitPrice.Currency);
        Assert.Equal(2500m, websiteDesign.LineTotal.Amount);

        Assert.Equal(20m, reloaded.TaxRate);
        Assert.Equal(5500m, reloaded.Totals.Subtotal.Amount);
        Assert.Equal(500m, reloaded.Totals.DiscountAmount.Amount);
        Assert.Equal(1000m, reloaded.Totals.TaxAmount.Amount);
        Assert.Equal(6000m, reloaded.Totals.Total.Amount);
    }
}