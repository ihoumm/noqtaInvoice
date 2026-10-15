using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer(
        Guid id,
        CustomerType type,
        string name,
        EmailAddress email,
        Address? address)
        : base(id)
    {
        Type = type;
        Name = RequireName(name);
        Email = email;
        Address = address;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // EF Core materialization constructor.
    // Address is an owned value object and cannot be constructor-bound by EF Core.
    private Customer()
    {
    }

    public CustomerType Type { get; private set; }

    public string Name { get; private set; }

    public EmailAddress Email { get; private set; }

    public Address? Address { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? DeactivatedAt { get; private set; }

    public static Customer Create(
        CustomerType type,
        string name,
        string email,
        Address? address = null)
    {
        return new Customer(
            Guid.NewGuid(),
            type,
            name,
            EmailAddress.Create(email),
            address);
    }

    public void Update(
        CustomerType type,
        string name,
        string email,
        Address? address)
    {
        EnsureActive();

        Type = type;
        Name = RequireName(name);
        Email = EmailAddress.Create(email);
        Address = address;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Customer is already inactive.");

        IsActive = false;
        DeactivatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new DomainException("Customer is already active.");

        IsActive = true;
        DeactivatedAt = null;
    }

    public CustomerInvoiceSnapshot CreateInvoiceSnapshot()
    {
        EnsureActive();

        return new CustomerInvoiceSnapshot(
            Name,
            Email,
            Address);
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException(
                "Inactive customer cannot be modified or used.");
    }

    private static string RequireName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");

        return name.Trim();
    }
}