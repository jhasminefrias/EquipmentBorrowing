using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnEquipmentResult> ReturnAsync(
        ReturnEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        Borrowing? borrowing = await _borrowingRepository.GetByIdAsync(request.BorrowingId, cancellationToken);
        if (borrowing is null)
        {
            return ReturnEquipmentResult.Failure("Borrowing record not found.");
        }

        if (borrowing.Status == BorrowingStatus.Returned)
        {
            return ReturnEquipmentResult.Failure("Borrowing has already been returned.");
        }

        if (request.ReturnDate < borrowing.DateBorrowed)
        {
            return ReturnEquipmentResult.Failure("Return date cannot be earlier than the borrowed date.");
        }

        Equipment? equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
        if (equipment is null)
        {
            return ReturnEquipmentResult.Failure("Equipment record not found.");
        }

        borrowing.MarkReturned(request.ReturnDate);
        equipment.MarkAvailable();

        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ReturnEquipmentResult.Success(borrowing);
    }
}
