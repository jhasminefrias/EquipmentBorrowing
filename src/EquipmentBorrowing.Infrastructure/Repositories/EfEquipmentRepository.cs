using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfEquipmentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Equipment?> GetByAssetTagAsync(string assetTag, CancellationToken cancellationToken = default)
    {
        string normalized = assetTag.Trim().ToLower();
        return await _context.Equipment
            .SingleOrDefaultAsync(e => e.AssetTag.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        // UC-02 query
        return await _context.Equipment
            .AsNoTracking()
            .Where(e => e.IsAvailable)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipment.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}