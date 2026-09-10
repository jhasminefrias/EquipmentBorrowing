using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings;
    private int _nextId;

    public InMemoryBorrowingRepository(IEnumerable<Borrowing>? initialBorrowings = null)
    {
        _borrowings = initialBorrowings?.ToList() ?? [];
        _nextId = _borrowings.Count > 0 ? _borrowings.Max(b => b.Id) + 1 : 1;
    }

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        int count = _borrowings.Count(borrowing =>
            borrowing.StudentId == studentId &&
            borrowing.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        if (borrowing.Id == 0)
        {
            borrowing.Id = _nextId++;
        }
        else if (borrowing.Id >= _nextId)
        {
            _nextId = borrowing.Id + 1;
        }

        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Borrowing? borrowing = _borrowings.SingleOrDefault(b => b.Id == id);
        return Task.FromResult(borrowing);
    }

    public Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> active = _borrowings
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToList();
        return Task.FromResult(active);
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        int index = _borrowings.FindIndex(b => b.Id == borrowing.Id);
        if (index >= 0)
        {
            _borrowings[index] = borrowing;
        }
        return Task.CompletedTask;
    }
}
