using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Tests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Create_CreatesActiveCustomer()
    {
        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(CustomerType.Company, customer.Type);
        Assert.Equal("ACME SARL", customer.Name);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Customer.Create(
                CustomerType.Company,
                "",
                "contact@acme.ma"));
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Customer.Create(
                CustomerType.Company,
                "ACME SARL",
                "invalid-email"));
    }

    [Fact]
    public void Deactivate_MakesCustomerInactive()
    {
        var customer = CreateCustomer();

        customer.Deactivate();

        Assert.False(customer.IsActive);
        Assert.NotNull(customer.DeactivatedAt);
    }

    [Fact]
    public void Reactivate_RestoresCustomer()
    {
        var customer = CreateCustomer();

        customer.Deactivate();
        customer.Reactivate();

        Assert.True(customer.IsActive);
        Assert.Null(customer.DeactivatedAt);
    }

    [Fact]
    public void InactiveCustomer_CannotBeUpdated()
    {
        var customer = CreateCustomer();

        customer.Deactivate();

        Assert.Throws<DomainException>(() =>
            customer.Update(
                CustomerType.Company,
                "Updated Name",
                "updated@example.com",
                null));
    }

    [Fact]
    public void CreateInvoiceSnapshot_PreservesCustomerData()
    {
        var address = new Address(
            "10 Avenue Mohammed V",
            "Casablanca",
            "20000",
            "Morocco");

        var customer = Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma",
            address);

        var snapshot = customer.CreateInvoiceSnapshot();

        Assert.Equal(customer.Name, snapshot.Name);
        Assert.Equal(customer.Email, snapshot.Email);
        Assert.Equal(customer.Address, snapshot.Address);
    }

    [Fact]
    public void InactiveCustomer_CannotCreateInvoiceSnapshot()
    {
        var customer = CreateCustomer();

        customer.Deactivate();

        Assert.Throws<DomainException>(
            () => customer.CreateInvoiceSnapshot());
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            CustomerType.Company,
            "ACME SARL",
            "contact@acme.ma");
    }
}