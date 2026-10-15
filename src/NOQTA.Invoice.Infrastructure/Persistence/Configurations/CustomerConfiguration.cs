using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NOQTA.Invoice.Domain.Customers;
using NOQTA.Invoice.Domain.ValueObjects;

namespace NOQTA.Invoice.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(x => x.Street)
                    .HasColumnName("AddressStreet")
                    .HasMaxLength(300)
                    .IsRequired();

                address.Property(x => x.City)
                    .HasColumnName("AddressCity")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.PostalCode)
                    .HasColumnName("AddressPostalCode")
                    .HasMaxLength(30);

                address.Property(x => x.Country)
                    .HasColumnName("AddressCountry")
                    .HasMaxLength(100)
                    .IsRequired();
            });

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.DeactivatedAt);
    }
}