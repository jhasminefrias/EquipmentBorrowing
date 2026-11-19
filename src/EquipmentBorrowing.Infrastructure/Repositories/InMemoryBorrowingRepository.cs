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
            .OrderByDescending(b => b.DateBorrowed)
            .ToList();
        return Task.FromResult(active);
    }

    public Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> list = _borrowings
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        string? sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Borrowing> query = _borrowings;

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "expectedreturn" or "returndate" => ascending
                ? query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.ExpectedReturnDate).ThenByDescending(b => b.Id),
            _ => ascending
                ? query.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.DateBorrowed).ThenByDescending(b => b.Id)
        };

        return Task.FromResult<IReadOnlyList<Borrowing>>(query.ToList());
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