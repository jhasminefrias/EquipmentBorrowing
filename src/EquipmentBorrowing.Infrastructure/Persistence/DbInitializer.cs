using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(EquipmentBorrowingDbContext context)
    {
        await context.Database.MigrateAsync();

        // Check if data already exists to avoid duplicate entries
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
        {
            return;
        }

        // 1. Initial Students
        var students = new[]
        {
            new Student(0, "2026-0001", "Jhasmine Frias", "jhasmine@example.edu", isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0002", "Hannah Rapal", "hannah@example.edu", isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0003", "Wendell Pasquil", null, isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0004", "Shanice Palasan", "shanice@example.edu", isAllowedToBorrow: false, maximumActiveBorrowings: 3), // Ineligible student for testing
            new Student(0, "2026-0005", "Airesh Abcd", null, isAllowedToBorrow: true, maximumActiveBorrowings: 3)
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // 2. Initial Equipment
        var equipment = new[]
        {
            new Equipment(0, "LAB-CAM-001", "Canon DSLR Camera", "High resolution camera with 18-55mm lens", isAvailable: true),
            new Equipment(0, "LAB-MIC-001", "Wireless Lavalier Microphone", "Dual channel wireless lapel mic", isAvailable: false),
            new Equipment(0, "LAB-PROJ-001", "Portable Projector", "1080p HDMI LED projector", isAvailable: true),
            new Equipment(0, "LAB-TRP-001", "Camera Tripod Stand", "Aluminum 60-inch tripod", isAvailable: true),
            new Equipment(0, "LAB-REC-001", "Digital Audio Recorder", "Stereo handheld field recorder", isAvailable: true),
            new Equipment(0, "LAB-LAP-001", "Dell XPS 15 Laptop", "Intel i7, 16GB RAM for lab computing", isAvailable: true),
            new Equipment(0, "LAB-TAB-001", "Wacom Drawing Tablet", "Graphic tablet with stylus", isAvailable: true)
        };

        await context.Equipment.AddRangeAsync(equipment);
        await context.SaveChangesAsync();

        // 3. Initial Active Borrowing (Wireless Mic borrowed by Jhasmine)
        Student jhasmine = await context.Students.FirstAsync(s => s.StudentNumber == "2026-0001");
        Equipment mic = await context.Equipment.FirstAsync(e => e.AssetTag == "LAB-MIC-001");

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var activeBorrowing = new Borrowing(
            0,
            jhasmine.Id,
            mic.Id,
            today.AddDays(-2),
            today.AddDays(5));

        await context.Borrowings.AddAsync(activeBorrowing);

        // 4. Historical Returned Borrowing (Projector borrowed & returned by Hannah)
        Student hannah = await context.Students.FirstAsync(s => s.StudentNumber == "2026-0002");
        Equipment proj = await context.Equipment.FirstAsync(e => e.AssetTag == "LAB-PROJ-001");

        var pastBorrowing = new Borrowing(
            0,
            hannah.Id,
            proj.Id,
            today.AddDays(-10),
            today.AddDays(-3));
        pastBorrowing.MarkReturned(today.AddDays(-4));

        await context.Borrowings.AddAsync(pastBorrowing);
        await context.SaveChangesAsync();
    }
}