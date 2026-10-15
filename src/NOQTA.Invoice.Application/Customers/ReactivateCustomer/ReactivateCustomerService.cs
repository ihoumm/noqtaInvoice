using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Customers.ReactivateCustomer;

public sealed class ReactivateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateCustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReactivateCustomerResult> ExecuteAsync(
        ReactivateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customer =
            await _customerRepository.GetByIdAsync(
                command.CustomerId,
                cancellationToken);

        if (customer is null)
        {
            throw new InvalidOperationException(
                $"Customer '{command.CustomerId}' was not found.");
        }

        customer.Reactivate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new ReactivateCustomerResult(
            customer.Id,
            customer.Name);
    }
}