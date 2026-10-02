using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(EquipmentBorrowingDbContext context)
    {
        // Ensure database and tables are created via migrations
        await context.Database.MigrateAsync();

        // Check if data is already seeded
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
        {
            return; // Database already contains seed data
        }

        // Seed initial students
        var students = new[]
        {
            new Student(0, "2026-0001", "Jhasmine Frias", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(0, "2026-0002", "Hannah Rapal", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(0, "2026-0003", "Wendell Pasquil", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(0, "2026-0004", "Shanice Palasan", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(0, "2026-0005", "Airesh Abcd", isAllowedToBorrow: true, maximumActiveBorrowings: 2)
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // Seed initial equipment
        var equipment = new[]
        {
            new Equipment(0, "LAB-CAM-001", "Digital Camera", isAvailable: true),
            new Equipment(0, "LAB-MIC-001", "Wireless Microphone", isAvailable: false),
            new Equipment(0, "LAB-PROJ-001", "Portable Projector", isAvailable: true),
            new Equipment(0, "LAB-TRP-001", "Heavy-Duty Camera Tripod", isAvailable: true),
            new Equipment(0, "LAB-REC-001", "Digital Audio Field Recorder", isAvailable: true),
            new Equipment(0, "LAB-LAP-001", "Dell XPS 15 Laptop", isAvailable: true),
            new Equipment(0, "LAB-LAP-002", "MacBook Pro 14\"", isAvailable: true),
            new Equipment(0, "LAB-TAB-001", "Wacom Drawing Tablet", isAvailable: true)
        };

        await context.Equipment.AddRangeAsync(equipment);
        await context.SaveChangesAsync();

        // Seed one active borrowing (Wireless Microphone borrowed by Jhasmine)
        Student? firstStudent = await context.Students.FirstAsync();
        Equipment? mic = await context.Equipment.FirstAsync(e => e.AssetTag == "LAB-MIC-001");

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var initialBorrowing = new Borrowing(
            0,
            firstStudent.Id,
            mic.Id,
            today.AddDays(-2),
            today.AddDays(5));

        await context.Borrowings.AddAsync(initialBorrowing);
        await context.SaveChangesAsync();
    }
}