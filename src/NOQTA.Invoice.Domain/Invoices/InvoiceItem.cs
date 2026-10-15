using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Invoices;

public sealed class InvoiceItem : Entity
{
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    public Money LineTotal =>
        UnitPrice.Multiply(Quantity);

    // EF Core materialization constructor.
    private InvoiceItem()
    {
    }

    private InvoiceItem(
        Guid id,
        string description,
        decimal quantity,
        Money unitPrice)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Invoice item description is required.");

        if (quantity <= 0)
            throw new DomainException("Invoice item quantity must be greater than zero.");

        if (unitPrice.Amount < 0)
            throw new DomainException("Invoice item unit price cannot be negative.");

        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static InvoiceItem Create(
        string description,
        decimal quantity,
        Money unitPrice)
    {
        return new InvoiceItem(
            Guid.NewGuid(),
            description,
            quantity,
            unitPrice);
    }

    public void Update(
        string description,
        decimal quantity,
        Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Invoice item description is required.");

        if (quantity <= 0)
            throw new DomainException("Invoice item quantity must be greater than zero.");

        if (unitPrice.Amount < 0)
            throw new DomainException("Invoice item unit price cannot be negative.");

        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}