using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var students = new[]
{
    new Student(1, "2026-0001", "Alex Reyes", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
    new Student(2, "2026-0002", "Jamie Santos", isAllowedToBorrow: false, maximumActiveBorrowings: 2)
};

var equipment = new[]
{
    new Equipment(1, "LAB-CAM-001", "Digital Camera", isAvailable: true),
    new Equipment(2, "LAB-MIC-001", "Wireless Microphone", isAvailable: false)
};

var studentRepository = new InMemoryStudentRepository(students);
var equipmentRepository = new InMemoryEquipmentRepository(equipment);
var borrowingRepository = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository);

DateOnly today = DateOnly.FromDateTime(DateTime.Today);

await RunDemoAsync(
    "Successful case: allowed student borrows available equipment",
    new BorrowEquipmentRequest(1, 1, today, today.AddDays(7)));

await RunDemoAsync(
    "Failure case: equipment is not available",
    new BorrowEquipmentRequest(1, 2, today, today.AddDays(7)));

await RunDemoAsync(
    "Failure case: student is not allowed to borrow",
    new BorrowEquipmentRequest(2, 1, today, today.AddDays(7)));

async Task RunDemoAsync(string title, BorrowEquipmentRequest request)
{
    Console.WriteLine(title);

    BorrowEquipmentResult result = await service.BorrowAsync(request);
    Console.WriteLine(result.Succeeded ? "Result: Success" : "Result: Failed");
    Console.WriteLine($"Message: {result.Message}");

    if (result.Borrowing is not null)
    {
        Console.WriteLine($"Student Id: {result.Borrowing.StudentId}");
        Console.WriteLine($"Equipment Id: {result.Borrowing.EquipmentId}");
        Console.WriteLine($"Expected Return: {result.Borrowing.ExpectedReturnDate}");
    }

    Console.WriteLine();
}
