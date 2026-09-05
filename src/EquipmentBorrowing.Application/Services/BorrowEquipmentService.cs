using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class BorrowEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowEquipmentResult> BorrowAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        Student? student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return BorrowEquipmentResult.Failure("Student does not exist.");
        }

        if (!student.IsAllowedToBorrow)
        {
            return BorrowEquipmentResult.Failure("Student is not allowed to borrow equipment.");
        }

        Equipment? equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
        {
            return BorrowEquipmentResult.Failure("Equipment does not exist.");
        }

        if (!equipment.IsAvailable)
        {
            return BorrowEquipmentResult.Failure("Equipment is not available.");
        }

        int activeBorrowings = await _borrowingRepository.CountActiveByStudentIdAsync(student.Id, cancellationToken);
        if (activeBorrowings >= student.MaximumActiveBorrowings)
        {
            return BorrowEquipmentResult.Failure("Student has reached the maximum number of active borrowings.");
        }

        var borrowing = new Borrowing(
            id: 0,
            studentId: student.Id,
            equipmentId: equipment.Id,
            dateBorrowed: request.DateBorrowed,
            expectedReturnDate: request.ExpectedReturnDate);

        equipment.MarkBorrowed();

        await _borrowingRepository.AddAsync(borrowing, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return BorrowEquipmentResult.Success(borrowing);
    }
}
