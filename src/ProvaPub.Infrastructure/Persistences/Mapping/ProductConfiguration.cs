using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Infrastructure.Persistences.Mapping;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable($"{nameof(Product)}s");

        // Chave Primária com Identity
        builder.HasKey(e => e.Id)
        .HasName($"IX_{nameof(Product)}s_{nameof(Product.Id)}");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        // Índice único no Name (assumindo que nomes de produtos devem ser únicos)
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName($"UQ_{nameof(Product)}s_{nameof(Product.Name)}");

        // Ou apenas um índice para busca
        // builder.HasIndex(x => x.Name)
        //     .HasDatabaseName($"UQ_{nameof(Product)}s_{nameof(Product.Name)}");
    }
}