using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Infrastructure.Persistences.Mapping;

public sealed class RandomNumberConfiguration : IEntityTypeConfiguration<RandomNumber>
{
    public void Configure(EntityTypeBuilder<RandomNumber> builder)
    {
        builder.ToTable($"{nameof(RandomNumber.Number)}s", CheckConstraint());

        // Chave Primária com Identity
        builder.HasKey(e => e.Id)
            .HasName($"PK_{nameof(RandomNumber.Number)}s_{nameof(RandomNumber.Id)}");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired();

        // Índice único no Number - garante que não haverá duplicatas
        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName($"UQ_{nameof(RandomNumber.Number)}s_{nameof(RandomNumber.Number)}");
    }

    //Check constraint - garante que Number está no range válido (0-99)
    private static Action<TableBuilder<RandomNumber>> CheckConstraint()
    {
        return builder => builder.HasCheckConstraint(
            $"CK_{nameof(RandomNumber.Number)}s_{nameof(RandomNumber.Number)}_Range",
            $"[{nameof(RandomNumber.Number)}] >= 0 AND [{nameof(RandomNumber.Number)}] <= 99");
    }
}