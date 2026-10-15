using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InvoiceAggregate = NOQTA.Invoice.Domain.Invoices.Invoice;
using NOQTA.Invoice.Domain.Invoices;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceAggregate>
{
    public void Configure(EntityTypeBuilder<InvoiceAggregate> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.InvoiceNumber)
            .HasConversion(
                number => number.Value,
                value => InvoiceNumber.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(
                currency => currency.Code,
                value => Currency.Create(value))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CustomerEmail)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.OwnsOne(
            x => x.CustomerAddress,
            address =>
            {
                address.Property(x => x.Street)
                    .HasColumnName("CustomerAddressStreet")
                    .HasMaxLength(300)
                    .IsRequired();

                address.Property(x => x.City)
                    .HasColumnName("CustomerAddressCity")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.PostalCode)
                    .HasColumnName("CustomerAddressPostalCode")
                    .HasMaxLength(30);

                address.Property(x => x.Country)
                    .HasColumnName("CustomerAddressCountry")
                    .HasMaxLength(100)
                    .IsRequired();
            });

        builder.Property(x => x.TaxRate)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.ComplexProperty(
    x => x.DiscountAmount,
    money =>
    {
        money.Property(x => x.Amount)
            .HasColumnName("DiscountAmount")
            .HasPrecision(18, 2)
            .IsRequired();

        money.Property(x => x.Currency)
            .HasConversion(
                currency => currency.Code,
                value => Currency.Create(value))
            .HasColumnName("DiscountCurrency")
            .HasMaxLength(3)
            .IsRequired();
    });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IssuedAt);

        builder.Property(x => x.SentAt);

        builder.Property(x => x.PaidAt);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}