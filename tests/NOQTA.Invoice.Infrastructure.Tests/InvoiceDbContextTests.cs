using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Infrastructure.Persistence;
using InvoiceNumberValue = NOQTA.Invoice.Domain.Invoices.InvoiceNumber;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class InvoiceDbContextTests
{
    [Fact]
    public async Task Can_persist_and_reload_customer_with_address()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new InvoiceDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var customer = Customer.Create(
                CustomerType.Company,
                "Atlas Studio",
                "billing@atlas-studio.ma",
                new Address(
                    "12 Avenue Mohammed V",
                    "Agadir",
                    "80000",
                    "Morocco"));

            context.Set<Customer>().Add(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = new InvoiceDbContext(options))
        {
            var customer = await context.Set<Customer>()
                .SingleAsync();

            Assert.Equal(CustomerType.Company, customer.Type);
            Assert.Equal("Atlas Studio", customer.Name);
            Assert.Equal("billing@atlas-studio.ma", customer.Email.Value);

            Assert.NotNull(customer.Address);
            Assert.Equal("12 Avenue Mohammed V", customer.Address!.Street);
            Assert.Equal("Agadir", customer.Address.City);
            Assert.Equal("80000", customer.Address.PostalCode);
            Assert.Equal("Morocco", customer.Address.Country);
            Assert.True(customer.IsActive);
        }
    }

    [Fact]
    public async Task Can_persist_and_reload_invoice_with_items_and_money_values()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        var currency = Currency.MAD;

        await using (var context = new InvoiceDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var invoice = InvoiceAggregate.Create(
                InvoiceNumberValue.Create("INV-2026-0001"),
                InvoiceType.Invoice,
                currency,
                "Atlas Studio",
                "billing@atlas-studio.ma",
                new Address(
                    "12 Avenue Mohammed V",
                    "Agadir",
                    "80000",
                    "Morocco"));

            invoice.AddItem(
                "Brand Identity",
                2m,
                Money.Create(1500m, currency));

            invoice.AddItem(
                "Invoice Design",
                1m,
                Money.Create(750m, currency));

            invoice.SetTaxRate(20m);
            invoice.SetDiscount(Money.Create(250m, currency));

            context.Set<InvoiceAggregate>().Add(invoice);

            await context.SaveChangesAsync();
        }

        await using (var context = new InvoiceDbContext(options))
        {
            var invoice = await context.Set<InvoiceAggregate>()
                .Include(x => x.Items)
                .SingleAsync();

            Assert.Equal("INV-2026-0001", invoice.InvoiceNumber.Value);
            Assert.Equal(InvoiceType.Invoice, invoice.Type);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Equal("MAD", invoice.Currency.Code);

            Assert.Equal("Atlas Studio", invoice.CustomerName);
            Assert.Equal(
                "billing@atlas-studio.ma",
                invoice.CustomerEmail.Value);

            Assert.NotNull(invoice.CustomerAddress);
            Assert.Equal(
                "12 Avenue Mohammed V",
                invoice.CustomerAddress!.Street);

            Assert.Equal(20m, invoice.TaxRate);
            Assert.Equal(250m, invoice.DiscountAmount.Amount);
            Assert.Equal("MAD", invoice.DiscountAmount.Currency.Code);

            Assert.Equal(2, invoice.Items.Count);

            var items = invoice.Items
                .OrderBy(x => x.Description)
                .ToList();

            var designItem = items.Single(x => x.Description == "Invoice Design");

            Assert.Equal(1m, designItem.Quantity);
            Assert.Equal(750m, designItem.UnitPrice.Amount);
            Assert.Equal("MAD", designItem.UnitPrice.Currency.Code);
            Assert.Equal(750m, designItem.LineTotal.Amount);

            var identityItem = items.Single(x => x.Description == "Brand Identity");

            Assert.Equal(2m, identityItem.Quantity);
            Assert.Equal(1500m, identityItem.UnitPrice.Amount);
            Assert.Equal("MAD", identityItem.UnitPrice.Currency.Code);
            Assert.Equal(3000m, identityItem.LineTotal.Amount);

            var totals = invoice.Totals;

            Assert.Equal(3750m, totals.Subtotal.Amount);
            Assert.Equal(250m, totals.DiscountAmount.Amount);
            Assert.Equal(700m, totals.TaxAmount.Amount);
            Assert.Equal(4200m, totals.Total.Amount);

            Assert.Equal("MAD", totals.Subtotal.Currency.Code);
            Assert.Equal("MAD", totals.DiscountAmount.Currency.Code);
            Assert.Equal("MAD", totals.TaxAmount.Currency.Code);
            Assert.Equal("MAD", totals.Total.Currency.Code);
        }
    }

    [Fact]
    public async Task Can_persist_invoice_without_customer_address()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new InvoiceDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var invoice = InvoiceAggregate.Create(
                InvoiceNumberValue.Create("INV-2026-0002"),
                InvoiceType.Invoice,
                Currency.MAD,
                "No Address Client",
                "client@example.com");

            invoice.AddItem(
                "Consulting",
                1m,
                Money.Create(1000m, Currency.MAD));

            context.Set<InvoiceAggregate>().Add(invoice);

            await context.SaveChangesAsync();
        }

        await using (var context = new InvoiceDbContext(options))
        {
            var invoice = await context.Set<InvoiceAggregate>()
                .SingleAsync();

            Assert.Null(invoice.CustomerAddress);
            Assert.Equal("No Address Client", invoice.CustomerName);
            Assert.Equal("client@example.com", invoice.CustomerEmail.Value);
        }
    }
}
