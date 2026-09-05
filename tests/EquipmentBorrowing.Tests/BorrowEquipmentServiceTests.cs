using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public static class BorrowEquipmentServiceTests
{
    public static async Task BorrowAsync_ReturnsFailure_WhenStudentDoesNotExist()
    {
        var service = CreateService();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        BorrowEquipmentResult result = await service.BorrowAsync(
            new BorrowEquipmentRequest(999, 1, today, today.AddDays(7)));

        if (result.Succeeded)
        {
            throw new InvalidOperationException("Expected borrowing request to fail.");
        }
    }

    private static BorrowEquipmentService CreateService()
    {
        var students = new[]
        {
            new Student(1, "2026-0001", "Alex Reyes", isAllowedToBorrow: true, maximumActiveBorrowings: 2)
        };

        var equipment = new[]
        {
            new Equipment(1, "LAB-CAM-001", "Digital Camera", isAvailable: true)
        };

        return new BorrowEquipmentService(
            new InMemoryStudentRepository(students),
            new InMemoryEquipmentRepository(equipment),
            new InMemoryBorrowingRepository());
    }
}
