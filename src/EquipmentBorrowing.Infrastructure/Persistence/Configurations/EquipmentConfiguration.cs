using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AssetTag)
            .IsRequired()
            .HasMaxLength(30);

        // BR-05: Case-insensitive unique asset tag
        builder.HasIndex(e => e.AssetTag)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired(false)
            .HasMaxLength(250);

        builder.Property(e => e.IsAvailable)
            .IsRequired();
    }
}