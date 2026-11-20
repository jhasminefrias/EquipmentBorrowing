namespace EquipmentBorrowing.Domain;

public sealed class Student
{
    public Student(int id, string studentNumber, string fullName, bool isAllowedToBorrow)
        : this(id, studentNumber, fullName, null, isAllowedToBorrow, 3)
    {
    }

    public Student(int id, string studentNumber, string fullName, bool isAllowedToBorrow, int maximumActiveBorrowings)
        : this(id, studentNumber, fullName, null, isAllowedToBorrow, maximumActiveBorrowings)
    {
    }

    public Student(int id, string studentNumber, string fullName, string? contactInfo, bool isAllowedToBorrow, int maximumActiveBorrowings = 3)
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
        StudentNumber = studentNumber.Trim();
        FullName = fullName.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
        IsAllowedToBorrow = isAllowedToBorrow;
        MaximumActiveBorrowings = maximumActiveBorrowings;
    }

    public int Id { get; }

    public string StudentNumber { get; private set; }

    public string FullName { get; private set; }

    public string? ContactInfo { get; private set; }

    public bool IsAllowedToBorrow { get; private set; }

    public int MaximumActiveBorrowings { get; }

    public void UpdateInformation(string studentNumber, string fullName, string? contactInfo)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException("Student number is required.", nameof(studentNumber));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        StudentNumber = studentNumber.Trim();
        FullName = fullName.Trim();
        ContactInfo = string.IsNullOrWhiteSpace(contactInfo) ? null : contactInfo.Trim();
    }

    public void SuspendBorrowing()
    {
        IsAllowedToBorrow = false;
    }

    public void RestoreBorrowingPrivilege()
    {
        IsAllowedToBorrow = true;
    }
}