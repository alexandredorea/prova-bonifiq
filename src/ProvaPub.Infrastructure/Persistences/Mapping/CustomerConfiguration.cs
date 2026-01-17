using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Infrastructure.Persistences.Mapping;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable($"{nameof(Customer)}s");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        // Índices
        builder.HasKey(e => e.Id)
            .HasName($"IX_{nameof(Customer)}s_{nameof(Customer.Id)}");

        // Configuração do relacionamento com Orders
        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName($"FK_{nameof(Order)}s_{nameof(Customer)}s_{nameof(Customer.Id)}");
    }
}