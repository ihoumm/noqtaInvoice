using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Invoices;

public sealed class Invoice : Entity
{
    private readonly List<InvoiceItem> _items = new();

    private Invoice()
    {
    }

    private Invoice(
        Guid id,
        InvoiceNumber invoiceNumber,
        InvoiceType type,
        Currency currency,
        string customerName,
        string customerEmail,
        Address? customerAddress)
        : base(id)
    {
        InvoiceNumber = invoiceNumber;
        Type = type;
        Currency = currency;

        CustomerName = RequireValue(customerName, "Customer name");
        CustomerEmail = EmailAddress.Create(customerEmail);
        CustomerAddress = customerAddress;

        Status = InvoiceStatus.Draft;
        TaxRate = 0m;
        DiscountAmount = Money.Zero(currency);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public InvoiceNumber InvoiceNumber { get; private set; }

    public InvoiceType Type { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public Currency Currency { get; private set; }

    public string CustomerName { get; private set; }

    public EmailAddress CustomerEmail { get; private set; }

    public Address? CustomerAddress { get; private set; }

    public decimal TaxRate { get; private set; }

    public Money DiscountAmount { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? IssuedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public InvoiceTotals Totals
    {
        get
        {
            var subtotal = Money.Zero(Currency);

            foreach (var item in _items)
            {
                subtotal = subtotal.Add(item.LineTotal);
            }

            var taxableAmount = subtotal.Subtract(DiscountAmount);

            var taxAmount = Money.Create(
                taxableAmount.Amount * TaxRate / 100m,
                Currency);

            var total = taxableAmount.Add(taxAmount);

            return new InvoiceTotals(
                subtotal,
                DiscountAmount,
                taxAmount,
                total);
        }
    }

    public static Invoice Create(
        InvoiceNumber invoiceNumber,
        InvoiceType type,
        Currency currency,
        string customerName,
        string customerEmail,
        Address? customerAddress = null)
    {
        return new Invoice(
            Guid.NewGuid(),
            invoiceNumber,
            type,
            currency,
            customerName,
            customerEmail,
            customerAddress);
    }

    public void AddItem(
        string description,
        decimal quantity,
        Money unitPrice)
    {
        EnsureDraft();

        if (unitPrice.Currency != Currency)
            throw new DomainException(
                "Invoice item currency must match invoice currency.");

        _items.Add(
            InvoiceItem.Create(
                description,
                quantity,
                unitPrice));
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureDraft();

        var item = _items.FirstOrDefault(x => x.Id == itemId);

        if (item is null)
            throw new DomainException("Invoice item was not found.");

        _items.Remove(item);
    }

    public void SetTaxRate(decimal taxRate)
    {
        EnsureDraft();

        if (taxRate < 0)
            throw new DomainException("Tax rate cannot be negative.");

        TaxRate = taxRate;
    }

    public void SetDiscount(Money discountAmount)
    {
        EnsureDraft();

        if (discountAmount.Currency != Currency)
            throw new DomainException(
                "Discount currency must match invoice currency.");

        if (discountAmount.Amount < 0)
            throw new DomainException(
                "Discount amount cannot be negative.");

        if (discountAmount.Amount > CalculateSubtotal().Amount)
            throw new DomainException(
                "Discount cannot exceed invoice subtotal.");

        DiscountAmount = discountAmount;
    }

    public void Issue()
    {
        EnsureDraft();

        if (_items.Count == 0)
            throw new DomainException(
                "An invoice must contain at least one item before it can be issued.");

        if (Totals.Total.Amount < 0)
            throw new DomainException(
                "Invoice total cannot be negative.");

        Status = InvoiceStatus.Issued;
        IssuedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Issued)
            throw new DomainException(
                "Only issued invoices can be marked as sent.");

        Status = InvoiceStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Sent)
            throw new DomainException(
                "Only sent invoices can be marked as paid.");

        Status = InvoiceStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException(
                "A paid invoice cannot be cancelled.");

        if (Status == InvoiceStatus.Cancelled)
            throw new DomainException(
                "Invoice is already cancelled.");

        Status = InvoiceStatus.Cancelled;
    }

    private Money CalculateSubtotal()
    {
        var subtotal = Money.Zero(Currency);

        foreach (var item in _items)
        {
            subtotal = subtotal.Add(item.LineTotal);
        }

        return subtotal;
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException(
                "Only draft invoices can be modified.");
    }

    private static string RequireValue(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        return value.Trim();
    }
}