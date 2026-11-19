namespace EquipmentBorrowing.Domain;

public sealed class Equipment
{
    public Equipment(int id, string assetTag, string name, string? description, bool isAvailable)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            throw new ArgumentException("Asset tag is required.", nameof(assetTag));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name is required.", nameof(name));
        }

        Id = id;
        AssetTag = assetTag.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsAvailable = isAvailable;
    }

    public int Id { get; }

    public string AssetTag { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsAvailable { get; private set; }

    public void UpdateInformation(string assetTag, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            throw new ArgumentException("Asset tag is required.", nameof(assetTag));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name is required.", nameof(name));
        }

        AssetTag = assetTag.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void MarkBorrowed()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException("Equipment is already borrowed.");
        }

        IsAvailable = false;
    }

    public void MarkAvailable()
    {
        IsAvailable = true;
    }
}