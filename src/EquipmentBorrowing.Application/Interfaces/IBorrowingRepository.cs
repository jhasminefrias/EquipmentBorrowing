using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> SearchAsync(string? searchText, BorrowingStatus? statusFilter, string? sortBy, bool ascending, CancellationToken cancellationToken = default);

    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}