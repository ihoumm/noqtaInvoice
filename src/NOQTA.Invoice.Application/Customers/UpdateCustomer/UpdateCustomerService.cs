using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateCustomerResult> ExecuteAsync(
        UpdateCustomerCommand command,
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

        Address? address = null;

        var hasAddress =
            !string.IsNullOrWhiteSpace(command.Street) ||
            !string.IsNullOrWhiteSpace(command.City) ||
            !string.IsNullOrWhiteSpace(command.PostalCode) ||
            !string.IsNullOrWhiteSpace(command.Country);

        if (hasAddress)
        {
            address = new Address(
                command.Street ?? string.Empty,
                command.City ?? string.Empty,
                command.PostalCode ?? string.Empty,
                command.Country ?? string.Empty);
        }

        customer.Update(
            command.Type,
            command.Name,
            command.Email,
            address);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new UpdateCustomerResult(
            customer.Id,
            customer.Name);
    }
}