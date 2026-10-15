using NOQTA.Invoice.Domain.Common;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Domain.Tests.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void Create_StartsAsDraft()
    {
        var invoice = CreateInvoice();

        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Empty(invoice.Items);
    }

    [Fact]
    public void AddItem_CalculatesSubtotal()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            2,
            Money.Create(1500m, Currency.MAD));

        Assert.Equal(3000m, invoice.Totals.Subtotal.Amount);
    }

    [Fact]
    public void SetTaxRate_CalculatesTaxAndTotal()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            2,
            Money.Create(1500m, Currency.MAD));

        invoice.SetTaxRate(20m);

        Assert.Equal(3000m, invoice.Totals.Subtotal.Amount);
        Assert.Equal(600m, invoice.Totals.TaxAmount.Amount);
        Assert.Equal(3600m, invoice.Totals.Total.Amount);
    }

    [Fact]
    public void SetDiscount_ReducesTaxableAmountAndTotal()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            2,
            Money.Create(1500m, Currency.MAD));

        invoice.SetDiscount(
            Money.Create(500m, Currency.MAD));

        invoice.SetTaxRate(20m);

        Assert.Equal(3000m, invoice.Totals.Subtotal.Amount);
        Assert.Equal(500m, invoice.Totals.DiscountAmount.Amount);
        Assert.Equal(500m, invoice.Totals.TaxAmount.Amount);
        Assert.Equal(3000m, invoice.Totals.Total.Amount);
    }

    [Fact]
    public void Issue_WithoutItems_Throws()
    {
        var invoice = CreateInvoice();

        Assert.Throws<DomainException>(() => invoice.Issue());
    }

    [Fact]
    public void Issue_WithItems_ChangesStatusToIssued()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(5000m, Currency.MAD));

        invoice.Issue();

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.NotNull(invoice.IssuedAt);
    }

    [Fact]
    public void IssuedInvoice_CannotBeModified()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(5000m, Currency.MAD));

        invoice.Issue();

        Assert.Throws<DomainException>(
            () => invoice.AddItem(
                "Website",
                1,
                Money.Create(3000m, Currency.MAD)));
    }

    [Fact]
    public void AddItem_WithDifferentCurrency_Throws()
    {
        var invoice = CreateInvoice();

        Assert.Throws<DomainException>(
            () => invoice.AddItem(
                "Brand Identity",
                1,
                Money.Create(
                    5000m,
                    Currency.Create("EUR"))));
    }

    [Fact]
    public void NegativeQuantity_Throws()
    {
        var invoice = CreateInvoice();

        Assert.Throws<DomainException>(
            () => invoice.AddItem(
                "Brand Identity",
                -1,
                Money.Create(5000m, Currency.MAD)));
    }

    [Fact]
    public void NegativeUnitPrice_Throws()
    {
        var invoice = CreateInvoice();

        Assert.Throws<DomainException>(
            () => invoice.AddItem(
                "Brand Identity",
                1,
                Money.Create(-5000m, Currency.MAD)));
    }

    [Fact]
    public void NegativeTaxRate_Throws()
    {
        var invoice = CreateInvoice();

        Assert.Throws<DomainException>(
            () => invoice.SetTaxRate(-1m));
    }

    [Fact]
    public void DiscountGreaterThanSubtotal_Throws()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(1000m, Currency.MAD));

        Assert.Throws<DomainException>(
            () => invoice.SetDiscount(
                Money.Create(1001m, Currency.MAD)));
    }

    [Fact]
    public void Cancel_DraftInvoice_ChangesStatusToCancelled()
    {
        var invoice = CreateInvoice();

        invoice.Cancel();

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);
    }

    [Fact]
    public void Cancel_IssuedInvoice_ChangesStatusToCancelled()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(5000m, Currency.MAD));

        invoice.Issue();

        invoice.Cancel();

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);
    }

    [Fact]
    public void Cancel_SentInvoice_ChangesStatusToCancelled()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(5000m, Currency.MAD));

        invoice.Issue();
        invoice.MarkAsSent();

        invoice.Cancel();

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);
    }

    [Fact]
    public void Cancel_PaidInvoice_Throws()
    {
        var invoice = CreateInvoice();

        invoice.AddItem(
            "Brand Identity",
            1,
            Money.Create(5000m, Currency.MAD));

        invoice.Issue();
        invoice.MarkAsSent();
        invoice.MarkAsPaid();

        var exception =
            Assert.Throws<DomainException>(
                () => invoice.Cancel());

        Assert.Equal(
            "A paid invoice cannot be cancelled.",
            exception.Message);

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);
    }

    [Fact]
    public void Cancel_AlreadyCancelledInvoice_Throws()
    {
        var invoice = CreateInvoice();

        invoice.Cancel();

        var exception =
            Assert.Throws<DomainException>(
                () => invoice.Cancel());

        Assert.Equal(
            "Invoice is already cancelled.",
            exception.Message);

        Assert.Equal(
            InvoiceStatus.Cancelled,
            invoice.Status);
    }

    private static InvoiceAggregate CreateInvoice()
    {
        return InvoiceAggregate.Create(
            InvoiceNumber.Create("INV-2026-0001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "NOQTA Client",
            "client@example.com");
    }
}