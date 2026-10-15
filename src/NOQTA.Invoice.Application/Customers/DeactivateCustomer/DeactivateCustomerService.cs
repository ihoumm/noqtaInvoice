using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Application.Customers.DeactivateCustomer;

public sealed class DeactivateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeactivateCustomerResult> ExecuteAsync(
        DeactivateCustomerCommand command,
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

        customer.Deactivate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new DeactivateCustomerResult(
            customer.Id,
            customer.Name);
    }
}