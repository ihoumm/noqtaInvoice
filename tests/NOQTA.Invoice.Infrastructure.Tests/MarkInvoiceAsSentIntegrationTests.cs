using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using InvoiceNumberValue = NOQTA.Invoice.Domain.Invoices.InvoiceNumber;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class MarkInvoiceAsSentIntegrationTests
{
    [Fact]
    public async Task MarkAsSent_PersistsSentStatusAndSentAt()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<InvoiceDbContext>()
                .UseSqlite(connection)
                .Options;

        await using (var setupContext =
            new InvoiceDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        var invoice = InvoiceAggregate.Create(
            InvoiceNumberValue.Create("INV-TEST-000002"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "1 Rue Test",
                "Casablanca",
                "20000",
                "Morocco"));

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(
                1500m,
                Currency.MAD));

        invoice.Issue();

        await using (var createContext =
            new InvoiceDbContext(options))
        {
            createContext.Invoices.Add(invoice);

            await createContext.SaveChangesAsync();
        }

        await using (var updateContext =
            new InvoiceDbContext(options))
        {
            var repository =
                new InvoiceRepository(updateContext);

            var persistedInvoice =
                await repository.GetByIdAsync(
                    invoice.Id);

            Assert.NotNull(persistedInvoice);

            persistedInvoice!.MarkAsSent();

            await updateContext.SaveChangesAsync();
        }

        await using (var verificationContext =
            new InvoiceDbContext(options))
        {
            var repository =
                new InvoiceRepository(verificationContext);

            var persistedInvoice =
                await repository.GetByIdAsync(
                    invoice.Id);

            Assert.NotNull(persistedInvoice);

            Assert.Equal(
                InvoiceStatus.Sent,
                persistedInvoice!.Status);

            Assert.NotNull(
                persistedInvoice.SentAt);
        }
    }
}