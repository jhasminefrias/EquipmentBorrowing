using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        // Aggregate query: Translated to SELECT COUNT(*) in SQL
        return await _context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.Borrowings.AddAsync(borrowing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Tracking query: entity will be modified when returned
        return await _context.Borrowings
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        // Read-only query for display in the Active Borrowings list
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}