using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Domain.Company;
using NOQTA.Invoice.Domain.ValueObjects;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class CompanyProfileRepositoryIntegrationTests
{
    [Fact]
    public async Task GetAsync_WhenProfileExists_ReturnsProfile()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var profile = CompanyProfile.Create(
            "NOQTA Agency",
            "hello@noqta.ma",
            "+212600000000",
            new Address(
                "10 Avenue Mohammed V",
                "Agadir",
                "80000",
                "Morocco"),
            "ICE123456789",
            "logos/noqta.png");

        await dbContext.Set<CompanyProfile>()
            .AddAsync(profile);

        await dbContext.SaveChangesAsync();

        var repository = new CompanyProfileRepository(dbContext);

        var result = await repository.GetAsync();

        Assert.NotNull(result);
        Assert.Equal(profile.Id, result.Id);
        Assert.Equal("NOQTA Agency", result.LegalName);
        Assert.Equal("hello@noqta.ma", result.Email.Value);
        Assert.Equal("+212600000000", result.Phone);

        Assert.NotNull(result.Address);
        Assert.Equal(
            "10 Avenue Mohammed V",
            result.Address.Street);
        Assert.Equal(
            "Agadir",
            result.Address.City);
        Assert.Equal(
            "80000",
            result.Address.PostalCode);
        Assert.Equal(
            "Morocco",
            result.Address.Country);

        Assert.Equal("ICE123456789", result.TaxIdentifier);
        Assert.Equal("logos/noqta.png", result.LogoPath);
        Assert.Equal(profile.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task GetAsync_WhenNoProfileExists_ReturnsNull()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository = new CompanyProfileRepository(dbContext);

        var result = await repository.GetAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_WhenNoProfileExists_AddsProfile()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var repository = new CompanyProfileRepository(dbContext);

        var profile = CompanyProfile.Create(
            "NOQTA Agency",
            "hello@noqta.ma",
            "+212600000000",
            new Address(
                "10 Avenue Mohammed V",
                "Agadir",
                "80000",
                "Morocco"));

        await repository.SaveAsync(profile);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Set<CompanyProfile>()
            .SingleAsync();

        Assert.Equal(profile.Id, persisted.Id);
        Assert.Equal("NOQTA Agency", persisted.LegalName);
        Assert.Equal("hello@noqta.ma", persisted.Email.Value);
        Assert.Equal("+212600000000", persisted.Phone);
    }

    [Fact]
    public async Task SaveAsync_WhenProfileExists_UpdatesExistingProfile()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new InvoiceDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var original = CompanyProfile.Create(
            "NOQTA Agency",
            "hello@noqta.ma",
            "+212600000000",
            new Address(
                "10 Avenue Mohammed V",
                "Agadir",
                "80000",
                "Morocco"),
            "ICE123456789",
            "logos/noqta.png");

        await dbContext.Set<CompanyProfile>()
            .AddAsync(original);

        await dbContext.SaveChangesAsync();

        var repository = new CompanyProfileRepository(dbContext);

        original.Update(
            "NOQTA Agency SARL",
            "contact@noqta.ma",
            "+212611111111",
            new Address(
                "25 Boulevard Hassan II",
                "Casablanca",
                "20000",
                "Morocco"),
            "ICE987654321",
            "logos/noqta-new.png");

        await repository.SaveAsync(original);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Set<CompanyProfile>()
            .SingleAsync();

        Assert.Equal(original.Id, persisted.Id);
        Assert.Equal("NOQTA Agency SARL", persisted.LegalName);
        Assert.Equal("contact@noqta.ma", persisted.Email.Value);
        Assert.Equal("+212611111111", persisted.Phone);

        Assert.Equal(
            "25 Boulevard Hassan II",
            persisted.Address.Street);
        Assert.Equal(
            "Casablanca",
            persisted.Address.City);
        Assert.Equal(
            "20000",
            persisted.Address.PostalCode);
        Assert.Equal(
            "Morocco",
            persisted.Address.Country);

        Assert.Equal("ICE987654321", persisted.TaxIdentifier);
        Assert.Equal("logos/noqta-new.png", persisted.LogoPath);
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingProfileAddress()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new InvoiceDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        var profile = CompanyProfile.Create(
            "Le 34 Media",
            "contact@le34media.ma",
            "+212 631-68 05 88",
            new Address(
                "Old Street",
                "Inezgane",
                "86350",
                "Morocco"));

        await using (var context = new InvoiceDbContext(options))
        {
            var repository =
                new CompanyProfileRepository(context);

            await repository.SaveAsync(profile);
            await context.SaveChangesAsync();
        }

        var updatedAddress = new Address(
            "Avenue Moulay Ali Cherif Tarrast Inezgane",
            "Inezgane",
            "86350",
            "Morocco");

        profile.Update(
            profile.LegalName,
            profile.Email.Value,
            profile.Phone,
            updatedAddress,
            profile.TaxIdentifier,
            profile.LogoPath);

        await using (var context = new InvoiceDbContext(options))
        {
            var repository =
                new CompanyProfileRepository(context);

            await repository.SaveAsync(profile);
            await context.SaveChangesAsync();
        }

        await using (var context = new InvoiceDbContext(options))
        {
            var repository =
                new CompanyProfileRepository(context);

            var reloaded =
                await repository.GetAsync();

            Assert.NotNull(reloaded);
            Assert.Equal(
                "Avenue Moulay Ali Cherif Tarrast Inezgane",
                reloaded.Address.Street);
            Assert.Equal("Inezgane", reloaded.Address.City);
            Assert.Equal("86350", reloaded.Address.PostalCode);
            Assert.Equal("Morocco", reloaded.Address.Country);
        }
    }
}