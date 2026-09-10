namespace EquipmentBorrowing.Application.Services;

public sealed record ReturnEquipmentRequest(
    int BorrowingId,
    DateOnly ReturnDate);
