namespace NOQTA.Invoice.Application.Abstractions.Persistence;

public interface IInvoiceNumberGenerator
{
    Task<long> GetNextAsync(
        CancellationToken cancellationToken = default);
}