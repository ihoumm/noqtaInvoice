using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application;
using NOQTA.Invoice.Infrastructure;
using NOQTA.Invoice.Infrastructure.Persistence;
using NOQTA.Invoice.Desktop.Invoices;

namespace NOQTA.Invoice.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddApplication();

        var applicationDataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "NOQTA",
            "Invoice");

        Directory.CreateDirectory(applicationDataDirectory);

        var databasePath = Path.Combine(
            applicationDataDirectory,
            "noqta-invoice.db");

        var weasyPrintExecutablePath =
            ResolveWeasyPrintExecutablePath();

        var pdfStorageDirectory =
            Path.Combine(
                applicationDataDirectory,
                "PDF");

        services.AddInfrastructure(
            $"Data Source={databasePath}",
            weasyPrintExecutablePath,
            pdfStorageDirectory);

        services.AddTransient<MainWindow>();

        services.AddTransient<Customers.CreateCustomerWindow>();

        services.AddTransient<CreateInvoiceWindow>();

        services.AddTransient<InvoiceDetailsWindow>();

        services.AddTransient<EditDraftInvoiceWindow>();

        services.AddTransient<Company.CompanyProfileWindow>();

        _serviceProvider = services.BuildServiceProvider();

        await InitializeDatabaseAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException(
                "Service provider has not been initialized.");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override void OnExit(
        ExitEventArgs e)
    {
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }

    private static string ResolveWeasyPrintExecutablePath()
{
    var projectRoot =
        Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../"));

    var executablePath =
        Path.Combine(
            projectRoot,
            ".weasyprint-venv",
            "Scripts",
            "weasyprint.exe");

    if (!File.Exists(executablePath))
    {
        throw new FileNotFoundException(
            "The WeasyPrint executable was not found. " +
            "Configure the application with a valid WeasyPrint installation.",
            executablePath);
    }

    return executablePath;
}
}