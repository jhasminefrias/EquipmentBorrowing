namespace EquipmentBorrowing.Application.Services;

public sealed record BorrowEquipmentRequest(
    int StudentId,
    int EquipmentId,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);
