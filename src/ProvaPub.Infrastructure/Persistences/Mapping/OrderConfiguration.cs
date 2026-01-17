using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Infrastructure.Persistences.Mapping;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable($"{nameof(Order)}s");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.OrderDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relacionamento com Customer
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName($"FK_{nameof(Order)}s_{nameof(Customer)}s_{nameof(Customer.Id)}");

        // Índices
        builder.HasKey(e => e.Id)
            .HasName($"IX_{nameof(Order)}s_{nameof(Order.Id)}");

        builder.HasIndex(x => x.CustomerId)
            .HasDatabaseName($"IX_{nameof(Order)}s_{nameof(Order.CustomerId)}");
    }
}