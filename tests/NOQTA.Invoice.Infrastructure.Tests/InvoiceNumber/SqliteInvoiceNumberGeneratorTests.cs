using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.InvoiceNumberGeneration;

namespace NOQTA.Invoice.Infrastructure.Tests.InvoiceNumber;

public sealed class SqliteInvoiceNumberGeneratorTests
{
    [Fact]
    public async Task GetNextAsync_ReturnsSequentialGlobalNumbers()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<InvoiceDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var generator =
            new SqliteInvoiceNumberGenerator(
                dbContext);

        var first =
            await generator.GetNextAsync();

        var second =
            await generator.GetNextAsync();

        var third =
            await generator.GetNextAsync();

        Assert.Equal(1L, first);
        Assert.Equal(2L, second);
        Assert.Equal(3L, third);
    }

    [Fact]
    public async Task GetNextAsync_IsGlobalAcrossGeneratorInstances()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        connection.Open();

        var options =
            new DbContextOptionsBuilder<InvoiceDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var firstContext =
            new InvoiceDbContext(options);

        await using var secondContext =
            new InvoiceDbContext(options);

        await firstContext.Database.EnsureCreatedAsync();

        var firstGenerator =
            new SqliteInvoiceNumberGenerator(
                firstContext);

        var secondGenerator =
            new SqliteInvoiceNumberGenerator(
                secondContext);

        var first =
            await firstGenerator.GetNextAsync();

        var second =
            await secondGenerator.GetNextAsync();

        Assert.Equal(1L, first);
        Assert.Equal(2L, second);
    }
}