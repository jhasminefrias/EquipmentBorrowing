using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(20);

        // BR-01: Case-insensitive uniqueness enforcement
        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactInfo)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired();

        builder.Property(s => s.MaximumActiveBorrowings)
            .IsRequired()
            .HasDefaultValue(3);
    }
}