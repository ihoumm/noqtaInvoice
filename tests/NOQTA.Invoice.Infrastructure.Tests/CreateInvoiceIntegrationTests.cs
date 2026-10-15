using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application;
using NOQTA.Invoice.Application.Customers.CreateCustomer;
using NOQTA.Invoice.Application.Invoices.CreateInvoice;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Infrastructure;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.InvoiceNumberGeneration;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class CreateInvoiceIntegrationTests
{
[Fact]
public async Task CreateInvoiceService_PersistsInvoiceAndReloadsIt()
{
await using var connection =
new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var services = new ServiceCollection();

    services.AddApplication();

    services.AddDbContext<InvoiceDbContext>(
        options => options.UseSqlite(connection));

    services.AddScoped<
    ICustomerRepository,
    NOQTA.Invoice.Infrastructure.Persistence.Repositories.CustomerRepository>();

services.AddScoped<
    IInvoiceRepository,
    NOQTA.Invoice.Infrastructure.Persistence.Repositories.InvoiceRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<IInvoiceNumberGenerator, SqliteInvoiceNumberGenerator>();

    await using var serviceProvider = services.BuildServiceProvider();

    await using (var initializationScope = serviceProvider.CreateAsyncScope())
    {
        var dbContext =
            initializationScope.ServiceProvider
                .GetRequiredService<InvoiceDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    Guid customerId;

    await using (var scope = serviceProvider.CreateAsyncScope())
    {
        var createCustomerService =
            scope.ServiceProvider
                .GetRequiredService<CreateCustomerService>();

        var customerResult =
            await createCustomerService.ExecuteAsync(
                new CreateCustomerCommand(
                    "Company",
                    "ACME SARL",
                    "contact@acme.ma",
                    "123 Boulevard Mohammed V",
                    "Casablanca",
                    "20000",
                    "Morocco"));

        customerId = customerResult.CustomerId;
    }

    CreateInvoiceResult invoiceResult;

    await using (var scope = serviceProvider.CreateAsyncScope())
    {
        var createInvoiceService =
            scope.ServiceProvider
                .GetRequiredService<CreateInvoiceService>();

        invoiceResult =
            await createInvoiceService.ExecuteAsync(
                new CreateInvoiceCommand(
                    customerId,
                    "Invoice",
                    "MAD",
                    new[]
                    {
                        new CreateInvoiceItem(
                            "Brand Identity",
                            2m,
                            1500m),

                        new CreateInvoiceItem(
                            "Invoice Design",
                            1m,
                            750m)
                    }));
    }

    await using (var scope = serviceProvider.CreateAsyncScope())
    {
        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<InvoiceDbContext>();

        var invoice =
            await dbContext.Set<InvoiceAggregate>()
                .Include(x => x.Items)
                .SingleAsync(
                    x => x.Id == invoiceResult.InvoiceId);

        Assert.Equal(
            "Invoice ACME SARL - 000001",
            invoice.InvoiceNumber.Value);

        Assert.Equal("ACME SARL", invoice.CustomerName);
        Assert.Equal("contact@acme.ma", invoice.CustomerEmail.Value);

        Assert.NotNull(invoice.CustomerAddress);
        Assert.Equal("123 Boulevard Mohammed V", invoice.CustomerAddress!.Street);
        Assert.Equal("Casablanca", invoice.CustomerAddress.City);
        Assert.Equal("20000", invoice.CustomerAddress.PostalCode);
        Assert.Equal("Morocco", invoice.CustomerAddress.Country);

        Assert.Equal(2, invoice.Items.Count);

        var brandIdentity =
            invoice.Items.Single(x => x.Description == "Brand Identity");

        Assert.Equal(2m, brandIdentity.Quantity);
        Assert.Equal(1500m, brandIdentity.UnitPrice.Amount);
        Assert.Equal("MAD", brandIdentity.UnitPrice.Currency.Code);

        var invoiceDesign =
            invoice.Items.Single(x => x.Description == "Invoice Design");

        Assert.Equal(1m, invoiceDesign.Quantity);
        Assert.Equal(750m, invoiceDesign.UnitPrice.Amount);
        Assert.Equal("MAD", invoiceDesign.UnitPrice.Currency.Code);

        Assert.Equal(3750m, invoice.Totals.Subtotal.Amount);
        Assert.Equal("MAD", invoice.Totals.Subtotal.Currency.Code);
    }
}

}