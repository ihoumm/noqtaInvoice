using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NOQTA.Invoice.Infrastructure.Persistence;

public sealed class InvoiceDbContextFactory
    : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<InvoiceDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=noqta-invoice-design.db");

        return new InvoiceDbContext(
            optionsBuilder.Options);
    }
}