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
        return await _context.Borrowings
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        // UC-07: Student history
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        // UC-08: Equipment history
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        string? sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        // UC-09: Search, filter & sort per BR-17
        var query = _context.Borrowings.AsNoTracking().AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string term = searchText.Trim().ToLower();

            var matchingStudentIds = _context.Students
                .Where(s => s.FullName.ToLower().Contains(term) || s.StudentNumber.ToLower().Contains(term))
                .Select(s => s.Id);

            var matchingEquipmentIds = _context.Equipment
                .Where(e => e.Name.ToLower().Contains(term) || e.AssetTag.ToLower().Contains(term))
                .Select(e => e.Id);

            query = query.Where(b => matchingStudentIds.Contains(b.StudentId) || matchingEquipmentIds.Contains(b.EquipmentId));
        }

        query = sortBy?.ToLower() switch
        {
            "expectedreturn" or "due" => ascending
                ? query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            _ => ascending
                ? query.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}