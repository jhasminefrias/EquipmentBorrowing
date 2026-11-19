using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment;
    private int _nextId;

    public InMemoryEquipmentRepository(IEnumerable<Equipment> equipment)
    {
        _equipment = equipment.ToList();
        _nextId = _equipment.Count > 0 ? _equipment.Max(e => e.Id) + 1 : 1;
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Equipment? item = _equipment.SingleOrDefault(e => e.Id == id);
        return Task.FromResult(item);
    }

    public Task<Equipment?> GetByAssetTagAsync(string assetTag, CancellationToken cancellationToken = default)
    {
        string normalized = assetTag.Trim().ToLower();
        Equipment? item = _equipment.SingleOrDefault(e => e.AssetTag.ToLower() == normalized);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> list = _equipment.OrderBy(e => e.Name).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> available = _equipment
            .Where(e => e.IsAvailable)
            .OrderBy(e => e.Name)
            .ToList();
        return Task.FromResult(available);
    }

    public Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment.Add(equipment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        int index = _equipment.FindIndex(e => e.Id == equipment.Id);
        if (index >= 0)
        {
            _equipment[index] = equipment;
        }
        return Task.CompletedTask;
    }
}