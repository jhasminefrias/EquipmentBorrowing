using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record CheckOutMultipleRequest(
    int StudentId,
    IReadOnlyList<int> EquipmentIds,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);

public sealed record CheckOutMultipleResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<Borrowing> Borrowings);

public sealed class CheckOutMultipleEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutMultipleEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckOutMultipleResult> CheckOutAsync(
        CheckOutMultipleRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. BR-12: Must request at least two distinct items
        var distinctItemIds = request.EquipmentIds.Distinct().ToList();
        if (distinctItemIds.Count < 2)
        {
            return new CheckOutMultipleResult(false, "Multi-item checkout requires at least two distinct equipment items.", []);
        }

        // 2. BR-11: Expected return date must be later than checkout date
        if (request.ExpectedReturnDate <= request.DateBorrowed)
        {
            return new CheckOutMultipleResult(false, "Expected return date must be later than the checkout date.", []);
        }

        // 3. BR-08: Validate student existence and eligibility
        Student? student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return new CheckOutMultipleResult(false, "Student does not exist.", []);
        }

        if (!student.IsAllowedToBorrow)
        {
            return new CheckOutMultipleResult(false, "Student is restricted from borrowing equipment.", []);
        }

        // 4. BR-09: Validate active limit (existing active + requested items <= 3)
        int currentActiveCount = await _borrowingRepository.CountActiveByStudentIdAsync(student.Id, cancellationToken);
        if (currentActiveCount + distinctItemIds.Count > student.MaximumActiveBorrowings)
        {
            return new CheckOutMultipleResult(
                false,
                $"Borrowing limit exceeded: Student currently has {currentActiveCount} active loan(s). Requesting {distinctItemIds.Count} items exceeds the limit of {student.MaximumActiveBorrowings}.",
                []);
        }

        // 5. BR-15: Open an explicit atomic database transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var createdBorrowings = new List<Borrowing>();

            foreach (int equipmentId in distinctItemIds)
            {
                // Revalidate current availability inside transaction boundary
                Equipment? item = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);

                if (item is null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return new CheckOutMultipleResult(false, $"Equipment item with ID {equipmentId} does not exist.", []);
                }

                if (!item.IsAvailable)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return new CheckOutMultipleResult(false, $"Equipment '{item.Name}' [{item.AssetTag}] is already borrowed/unavailable.", []);
                }

                item.MarkBorrowed();
                await _equipmentRepository.UpdateAsync(item, cancellationToken);

                var borrowing = new Borrowing(
                    0,
                    student.Id,
                    item.Id,
                    request.DateBorrowed,
                    request.ExpectedReturnDate);

                await _borrowingRepository.AddAsync(borrowing, cancellationToken);
                createdBorrowings.Add(borrowing);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return new CheckOutMultipleResult(
                true,
                $"Successfully checked out {createdBorrowings.Count} equipment items for {student.FullName}.",
                createdBorrowings);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return new CheckOutMultipleResult(false, $"Database transaction failed and was rolled back: {ex.Message}", []);
        }
    }
}