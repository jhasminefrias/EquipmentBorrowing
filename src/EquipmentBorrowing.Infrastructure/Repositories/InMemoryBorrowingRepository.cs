using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = [];
    private int _nextId = 1;

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        int count = _borrowings.Count(borrowing =>
            borrowing.StudentId == studentId &&
            borrowing.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        var storedBorrowing = new Borrowing(
            _nextId++,
            borrowing.StudentId,
            borrowing.EquipmentId,
            borrowing.DateBorrowed,
            borrowing.ExpectedReturnDate);

        _borrowings.Add(storedBorrowing);
        return Task.CompletedTask;
    }
}
