using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Customers.CreateCustomer;
using NOQTA.Invoice.Application.Customers.GetCustomers;
using NOQTA.Invoice.Application.Invoices.CreateInvoice;
using NOQTA.Invoice.Application.Invoices.GetInvoices;
using NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;
using NOQTA.Invoice.Application.Invoices.IssueInvoice;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsSent;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsPaid;
using NOQTA.Invoice.Application.Company.GetCompanyProfile;
using NOQTA.Invoice.Application.Company.SaveCompanyProfile;
using NOQTA.Invoice.Application.Invoices.UpdateDraftInvoice;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;
using NOQTA.Invoice.Application.Invoices.CancelInvoice;
using NOQTA.Invoice.Application.Customers.UpdateCustomer;
using NOQTA.Invoice.Application.Customers.DeactivateCustomer;
using NOQTA.Invoice.Application.Customers.ReactivateCustomer;

namespace NOQTA.Invoice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateInvoiceService>();

        services.AddScoped<CreateCustomerService>();

        services.AddScoped<GetCustomersService>();

        services.AddScoped<GetInvoicesService>();

        services.AddScoped<GetInvoiceDetailsService>();

        services.AddScoped<IssueInvoiceService>();

        services.AddScoped<CancelInvoiceService>();

        services.AddScoped<MarkInvoiceAsSentService>();

        services.AddScoped<MarkInvoiceAsPaidService>();

        services.AddScoped<GetCompanyProfileService>();

        services.AddScoped<SaveCompanyProfileService>();

        services.AddScoped<UpdateDraftInvoiceService>();

        services.AddScoped<IInvoiceDocumentBuilder, InvoiceDocumentBuilder>();

        services.AddScoped<GenerateInvoicePdfService>();

        services.AddScoped<UpdateCustomerService>();

        services.AddScoped<DeactivateCustomerService>();

        services.AddScoped<ReactivateCustomerService>();

        return services;
    }
}