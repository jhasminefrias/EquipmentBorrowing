namespace EquipmentBorrowing.Domain;

public sealed class Equipment
{
    public Equipment(int id, string assetTag, string name, bool isAvailable)
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
        AssetTag = assetTag;
        Name = name;
        IsAvailable = isAvailable;
    }

    public int Id { get; }

    public string AssetTag { get; }

    public string Name { get; }

    public bool IsAvailable { get; private set; }

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
