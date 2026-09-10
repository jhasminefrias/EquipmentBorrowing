namespace EquipmentBorrowing.Domain;

public sealed class Borrowing
{
    public Borrowing(int id, int studentId, int equipmentId, DateOnly dateBorrowed, DateOnly expectedReturnDate)
    {
        if (expectedReturnDate < dateBorrowed)
        {
            throw new ArgumentException("Expected return date cannot be earlier than the borrowed date.", nameof(expectedReturnDate));
        }

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public int Id { get; set; }

    public int StudentId { get; }

    public int EquipmentId { get; }

    public DateOnly DateBorrowed { get; }

    public DateOnly ExpectedReturnDate { get; }

    public BorrowingStatus Status { get; private set; }

    public DateOnly? DateReturned { get; private set; }

    public void MarkReturned(DateOnly dateReturned)
    {
        if (Status == BorrowingStatus.Returned)
        {
            throw new InvalidOperationException("Borrowing has already been returned.");
        }

        if (dateReturned < DateBorrowed)
        {
            throw new ArgumentException("Return date cannot be earlier than the borrowed date.", nameof(dateReturned));
        }

        Status = BorrowingStatus.Returned;
        DateReturned = dateReturned;
    }
}
