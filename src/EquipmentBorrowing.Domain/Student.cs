namespace EquipmentBorrowing.Domain;

public sealed class Student
{
    public Student(int id, string studentNumber, string fullName, bool isAllowedToBorrow, int maximumActiveBorrowings)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException("Student number is required.", nameof(studentNumber));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (maximumActiveBorrowings < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumActiveBorrowings), "Maximum active borrowings must be at least 1.");
        }

        Id = id;
        StudentNumber = studentNumber;
        FullName = fullName;
        IsAllowedToBorrow = isAllowedToBorrow;
        MaximumActiveBorrowings = maximumActiveBorrowings;
    }

    public int Id { get; }

    public string StudentNumber { get; }

    public string FullName { get; }

    public bool IsAllowedToBorrow { get; private set; }

    public int MaximumActiveBorrowings { get; }

    public void SuspendBorrowing()
    {
        IsAllowedToBorrow = false;
    }

    public void RestoreBorrowingPrivilege()
    {
        IsAllowedToBorrow = true;
    }
}
