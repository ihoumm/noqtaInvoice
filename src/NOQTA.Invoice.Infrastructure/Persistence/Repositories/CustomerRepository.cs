using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.Customers;

namespace NOQTA.Invoice.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly InvoiceDbContext _dbContext;

    public CustomerRepository(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await _dbContext.Customers.AddAsync(
            customer,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .ToListAsync(cancellationToken);
    }
}