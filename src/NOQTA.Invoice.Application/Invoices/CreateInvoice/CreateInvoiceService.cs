using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Application.Invoices.CreateInvoice;

public sealed class CreateInvoiceService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceService(
        ICustomerRepository customerRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInvoiceResult> ExecuteAsync(
        CreateInvoiceCommand command,
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

        var customerSnapshot =
            customer.CreateInvoiceSnapshot();

        var invoiceType =
            ParseInvoiceType(command.InvoiceType);

        var currency =
            Currency.Create(command.CurrencyCode);

        var sequence =
            await _invoiceNumberGenerator.GetNextAsync(
                cancellationToken);

        var invoiceNumber =
            $"Invoice {customerSnapshot.Name} - {sequence:D6}";

        var invoice =
            InvoiceAggregate.Create(
                InvoiceNumber.Create(invoiceNumber),
                invoiceType,
                currency,
                customerSnapshot.Name,
                customerSnapshot.Email.ToString(),
                customerSnapshot.Address);

        foreach (var item in command.Items)
        {
            invoice.AddItem(
                item.Description,
                item.Quantity,
                Money.Create(
                    item.UnitPrice,
                    invoice.Currency));
        }

        await _invoiceRepository.AddAsync(
            invoice,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateInvoiceResult(
            invoice.Id,
            invoice.InvoiceNumber.Value);
    }

    private static InvoiceType ParseInvoiceType(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Invoice type is required.",
                nameof(value));
        }

        if (!Enum.TryParse<InvoiceType>(
                value,
                ignoreCase: true,
                out var invoiceType) ||
            !Enum.IsDefined(invoiceType))
        {
            throw new ArgumentException(
                $"Invalid invoice type '{value}'.",
                nameof(value));
        }

        return invoiceType;
    }
}