using NOQTA.Invoice.Domain.Common;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;

namespace NOQTA.Invoice.Domain.Tests.Invoices;

public sealed class MarkAsPaidTests
{
    [Fact]
    public void MarkAsPaid_FromSentInvoice_SetsStatusAndPaidAt()
    {
        var invoice = CreateSentInvoice();

        invoice.MarkAsPaid();

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);

        Assert.NotNull(invoice.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_FromDraftInvoice_ThrowsDomainException()
    {
        var invoice = CreateDraftInvoice();

        var exception = Assert.Throws<DomainException>(
            () => invoice.MarkAsPaid());

        Assert.Equal(
            "Only sent invoices can be marked as paid.",
            exception.Message);

        Assert.Equal(
            InvoiceStatus.Draft,
            invoice.Status);

        Assert.Null(invoice.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_FromIssuedInvoice_ThrowsDomainException()
    {
        var invoice = CreateIssuedInvoice();

        var exception = Assert.Throws<DomainException>(
            () => invoice.MarkAsPaid());

        Assert.Equal(
            "Only sent invoices can be marked as paid.",
            exception.Message);

        Assert.Equal(
            InvoiceStatus.Issued,
            invoice.Status);

        Assert.Null(invoice.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ThrowsDomainException()
    {
        var invoice = CreateSentInvoice();

        invoice.MarkAsPaid();

        var paidAt = invoice.PaidAt;

        var exception = Assert.Throws<DomainException>(
            () => invoice.MarkAsPaid());

        Assert.Equal(
            "Only sent invoices can be marked as paid.",
            exception.Message);

        Assert.Equal(
            InvoiceStatus.Paid,
            invoice.Status);

        Assert.NotNull(invoice.PaidAt);
        Assert.Equal(paidAt, invoice.PaidAt);
    }

    private static InvoiceAggregate  CreateDraftInvoice()
    {
        return InvoiceAggregate .Create(
            InvoiceNumber.Create("INV-TEST-PAID-001"),
            InvoiceType.Invoice,
            Currency.MAD,
            "ACME SARL",
            "contact@acme.ma",
            new Address(
                "1 Rue Test",
                "Casablanca",
                "20000",
                "Morocco"));
    }

    private static InvoiceAggregate  CreateIssuedInvoice()
    {
        var invoice = CreateDraftInvoice();

        invoice.AddItem(
            "Brand Identity",
            2m,
            Money.Create(
                1500m,
                Currency.MAD));

        invoice.Issue();

        return invoice;
    }

    private static InvoiceAggregate  CreateSentInvoice()
    {
        var invoice = CreateIssuedInvoice();

        invoice.MarkAsSent();

        return invoice;
    }
}