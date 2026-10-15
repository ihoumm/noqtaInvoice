using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Abstractions.Files;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Infrastructure.Persistence.Repositories;
using NOQTA.Invoice.Infrastructure.Pdf;
using NOQTA.Invoice.Infrastructure.Storage;
using NOQTA.Invoice.Infrastructure.Persistence.InvoiceNumberGeneration;

namespace NOQTA.Invoice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string weasyPrintExecutablePath,
        string pdfStorageDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            weasyPrintExecutablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            pdfStorageDirectory);

        services.AddDbContext<InvoiceDbContext>(
            options => options.UseSqlite(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICompanyProfileRepository, CompanyProfileRepository>();

        services.AddScoped<IInvoicePdfRenderer>(
            _ => new WeasyPrintInvoicePdfRenderer(
                weasyPrintExecutablePath));

        services.AddScoped<IFileStorage>(
            _ => new LocalFileStorage(
                pdfStorageDirectory));
        
        services.AddScoped<IInvoiceNumberGenerator, SqliteInvoiceNumberGenerator>();

        return services;
    }
}