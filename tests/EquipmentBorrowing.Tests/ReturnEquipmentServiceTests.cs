using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public static class ReturnEquipmentServiceTests
{
    public static async Task ReturnAsync_ReturnsSuccess_WhenActiveBorrowingExists()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var (service, equipmentRepo, borrowingRepo) = CreateService(today);

        var request = new ReturnEquipmentRequest(1, today);
        ReturnEquipmentResult result = await service.ReturnAsync(request);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Expected return to succeed, but failed: {result.Message}");
        }

        Borrowing? borrowing = await borrowingRepo.GetByIdAsync(1);
        if (borrowing?.Status != BorrowingStatus.Returned)
        {
            throw new InvalidOperationException("Expected borrowing status to be Returned.");
        }

        Equipment? equipment = await equipmentRepo.GetByIdAsync(1);
        if (equipment?.IsAvailable != true)
        {
            throw new InvalidOperationException("Expected equipment to be marked available.");
        }
    }

    public static async Task ReturnAsync_ReturnsFailure_WhenBorrowingDoesNotExist()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var (service, _, _) = CreateService(today);

        var request = new ReturnEquipmentRequest(999, today);
        ReturnEquipmentResult result = await service.ReturnAsync(request);

        if (result.Succeeded)
        {
            throw new InvalidOperationException("Expected non-existent borrowing return to fail.");
        }
    }

    public static async Task ReturnAsync_ReturnsFailure_WhenBorrowingAlreadyReturned()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var (service, _, _) = CreateService(today);

        var request = new ReturnEquipmentRequest(1, today);
        await service.ReturnAsync(request);

        ReturnEquipmentResult secondResult = await service.ReturnAsync(request);
        if (secondResult.Succeeded)
        {
            throw new InvalidOperationException("Expected repeated return on already returned item to fail.");
        }
    }

    private static (ReturnEquipmentService Service, InMemoryEquipmentRepository EquipmentRepo, InMemoryBorrowingRepository BorrowingRepo) CreateService(DateOnly today)
    {
        var equipment = new[]
        {
            new Equipment(1, "LAB-CAM-001", "Digital Camera", isAvailable: false)
        };

        var borrowings = new[]
        {
            new Borrowing(1, 1, 1, today.AddDays(-2), today.AddDays(5))
        };

        var equipmentRepo = new InMemoryEquipmentRepository(equipment);
        var borrowingRepo = new InMemoryBorrowingRepository(borrowings);
        var service = new ReturnEquipmentService(borrowingRepo, equipmentRepo);

        return (service, equipmentRepo, borrowingRepo);
    }
}
