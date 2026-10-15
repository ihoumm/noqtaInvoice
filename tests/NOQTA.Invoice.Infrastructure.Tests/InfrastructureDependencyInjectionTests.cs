using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Abstractions.Files;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Infrastructure;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;
using NOQTA.Invoice.Infrastructure.Pdf;
using NOQTA.Invoice.Infrastructure.Storage;

namespace NOQTA.Invoice.Infrastructure.Tests;

public sealed class InfrastructureDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersCompanyProfileRepository()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(
            "Data Source=:memory:",
            "weasyprint.exe",
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Test"));

        using var serviceProvider =
            services.BuildServiceProvider();

        var repository =
            serviceProvider.GetRequiredService<ICompanyProfileRepository>();

        Assert.IsType<CompanyProfileRepository>(repository);
    }

    [Fact]
    public void AddInfrastructure_RegistersInvoicePdfRenderer()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(
            "Data Source=:memory:",
            "weasyprint.exe",
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Test"));

        using var serviceProvider =
            services.BuildServiceProvider();

        var renderer =
            serviceProvider.GetRequiredService<IInvoicePdfRenderer>();

        Assert.IsType<WeasyPrintInvoicePdfRenderer>(renderer);
    }

    [Fact]
    public void AddInfrastructure_RegistersFileStorage()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(
            "Data Source=:memory:",
            "weasyprint.exe",
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Test"));

        using var serviceProvider =
            services.BuildServiceProvider();

        var storage =
            serviceProvider.GetRequiredService<IFileStorage>();

        Assert.IsType<LocalFileStorage>(storage);
    }
}