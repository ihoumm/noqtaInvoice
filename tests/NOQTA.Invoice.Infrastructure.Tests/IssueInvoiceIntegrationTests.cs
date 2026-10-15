using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;

using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

using InvoiceNumberValue = NOQTA.Invoice.Domain.Invoices.InvoiceNumber;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class IssueInvoiceIntegrationTests
{
    [Fact]
    public async Task IssueInvoice_PersistsIssuedStatusAndIssuedAt()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid invoiceId;

        await using (var dbContext = new InvoiceDbContext(options))
        {
            await dbContext.Database.EnsureCreatedAsync();

            var invoice = InvoiceAggregate.Create(
                InvoiceNumberValue.Create("INV-2026-0100"),
                InvoiceType.Invoice,
                Currency.MAD,
                "ACME SARL",
                "contact@acme.ma");

            invoice.AddItem(
                "Brand Identity",
                2m,
                Money.Create(1500m, Currency.MAD));

            await dbContext.Invoices.AddAsync(invoice);
            await dbContext.SaveChangesAsync();

            invoiceId = invoice.Id;

            Assert.Equal(
                InvoiceStatus.Draft,
                invoice.Status);

            Assert.Null(invoice.IssuedAt);
        }

        await using (var dbContext = new InvoiceDbContext(options))
        {
            var repository =
                new InvoiceRepository(dbContext);

            var invoice =
                await repository.GetByIdAsync(
                    invoiceId);

            Assert.NotNull(invoice);

            Assert.Equal(
                InvoiceStatus.Draft,
                invoice.Status);

            Assert.Null(invoice.IssuedAt);

            invoice.Issue();

            await dbContext.SaveChangesAsync();
        }

        await using (var verificationContext =
            new InvoiceDbContext(options))
        {
            var repository =
                new InvoiceRepository(
                    verificationContext);

            var reloaded =
                await repository.GetByIdAsync(
                    invoiceId);

            Assert.NotNull(reloaded);

            Assert.Equal(
                InvoiceStatus.Issued,
                reloaded.Status);

            Assert.NotNull(reloaded.IssuedAt);
        }
    }
}