using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.StudentId)
            .IsRequired();

        builder.Property(b => b.EquipmentId)
            .IsRequired();

        builder.Property(b => b.DateBorrowed)
            .IsRequired();

        builder.Property(b => b.ExpectedReturnDate)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.DateReturned)
            .IsRequired(false);

        // Foreign keys preserving referential integrity
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.StudentId, b.Status });

        // BR-10 / BR-16: PostgreSQL partial index ensuring only ONE active borrowing per equipment item
        builder.HasIndex(b => b.EquipmentId)
            .IsUnique()
            .HasFilter("\"Status\" = 0");
    }
}