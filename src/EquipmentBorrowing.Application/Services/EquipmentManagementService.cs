using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class EquipmentManagementService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentManagementService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task<(bool Succeeded, string Message, Equipment? Equipment)> RegisterEquipmentAsync(
        string assetTag,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assetTag) || string.IsNullOrWhiteSpace(name))
        {
            return (false, "Asset tag and equipment name are required.", null);
        }

        Equipment? existing = await _equipmentRepository.GetByAssetTagAsync(assetTag, cancellationToken);
        if (existing is not null)
        {
            return (false, $"Asset tag '{assetTag.Trim()}' is already registered.", null);
        }

        var equipment = new Equipment(0, assetTag, name, description, isAvailable: true);
        await _equipmentRepository.AddAsync(equipment, cancellationToken);

        return (true, "Equipment registered successfully.", equipment);
    }

    public async Task<(bool Succeeded, string Message, Equipment? Equipment)> UpdateEquipmentInfoAsync(
        int equipmentId,
        string assetTag,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        Equipment? equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null)
        {
            return (false, "Equipment record not found.", null);
        }

        if (string.IsNullOrWhiteSpace(assetTag) || string.IsNullOrWhiteSpace(name))
        {
            return (false, "Asset tag and name cannot be blank.", null);
        }

        Equipment? existing = await _equipmentRepository.GetByAssetTagAsync(assetTag, cancellationToken);
        if (existing is not null && existing.Id != equipmentId)
        {
            return (false, $"Asset tag '{assetTag.Trim()}' is already used by another item.", null);
        }

        equipment.UpdateInformation(assetTag, name, description);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return (true, "Equipment information updated successfully.", equipment);
    }
}