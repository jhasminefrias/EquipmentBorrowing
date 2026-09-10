using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record ReturnEquipmentResult(
    bool Succeeded,
    string Message,
    Borrowing? Borrowing)
{
    public static ReturnEquipmentResult Success(Borrowing borrowing)
    {
        return new ReturnEquipmentResult(true, "Equipment successfully returned.", borrowing);
    }

    public static ReturnEquipmentResult Failure(string message)
    {
        return new ReturnEquipmentResult(false, message, null);
    }
}
