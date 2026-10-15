using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Domain.Customers;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Infrastructure.Persistence;

public sealed class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(
        DbContextOptions<InvoiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<InvoiceAggregate> Invoices => Set<InvoiceAggregate>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InvoiceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}