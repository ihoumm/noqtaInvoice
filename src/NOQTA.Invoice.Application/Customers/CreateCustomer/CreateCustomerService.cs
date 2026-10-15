using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Customers.CreateCustomer;

public sealed class CreateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCustomerResult> ExecuteAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customerType = ParseCustomerType(
            command.CustomerType);

        var address = CreateAddress(command);

        var customer = Customer.Create(
            customerType,
            command.Name,
            command.Email,
            address);

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateCustomerResult(
            customer.Id,
            customer.Name);
    }

    private static CustomerType ParseCustomerType(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Customer type is required.",
                nameof(value));
        }

        if (!Enum.TryParse<CustomerType>(
                value,
                ignoreCase: true,
                out var customerType) ||
            !Enum.IsDefined(customerType))
        {
            throw new ArgumentException(
                $"Invalid customer type '{value}'.",
                nameof(value));
        }

        return customerType;
    }

    private static Address? CreateAddress(
        CreateCustomerCommand command)
    {
        var hasAddress =
            !string.IsNullOrWhiteSpace(command.Street) ||
            !string.IsNullOrWhiteSpace(command.City) ||
            !string.IsNullOrWhiteSpace(command.PostalCode) ||
            !string.IsNullOrWhiteSpace(command.Country);

        if (!hasAddress)
            return null;

        return new Address(
            command.Street ?? string.Empty,
            command.City ?? string.Empty,
            command.PostalCode ?? string.Empty,
            command.Country ?? string.Empty);
    }
}