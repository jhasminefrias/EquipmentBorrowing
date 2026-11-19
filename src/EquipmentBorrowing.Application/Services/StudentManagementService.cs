using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class StudentManagementService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManagementService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<(bool Succeeded, string Message, Student? Student)> RegisterStudentAsync(
        string studentNumber,
        string fullName,
        string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "Student number and full name are required.", null);
        }

        Student? existing = await _studentRepository.GetByStudentNumberAsync(studentNumber, cancellationToken);
        if (existing is not null)
        {
            return (false, $"Student number '{studentNumber.Trim()}' is already registered.", null);
        }

        var student = new Student(0, studentNumber, fullName, contactInfo, isAllowedToBorrow: true, maximumActiveBorrowings: 3);
        await _studentRepository.AddAsync(student, cancellationToken);

        return (true, "Student registered successfully.", student);
    }

    public async Task<(bool Succeeded, string Message, Student? Student)> UpdateStudentInfoAsync(
        int studentId,
        string studentNumber,
        string fullName,
        string? contactInfo,
        CancellationToken cancellationToken = default)
    {
        Student? student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            return (false, "Student record not found.", null);
        }

        if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "Student number and full name cannot be blank.", null);
        }

        Student? existing = await _studentRepository.GetByStudentNumberAsync(studentNumber, cancellationToken);
        if (existing is not null && existing.Id != studentId)
        {
            return (false, $"Student number '{studentNumber.Trim()}' is used by another student.", null);
        }

        student.UpdateInformation(studentNumber, fullName, contactInfo);
        await _studentRepository.UpdateAsync(student, cancellationToken);

        return (true, "Student information updated successfully.", student);
    }

    public async Task<(bool Succeeded, string Message)> ChangeEligibilityAsync(
        int studentId,
        bool allowBorrowing,
        CancellationToken cancellationToken = default)
    {
        Student? student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            return (false, "Student record not found.");
        }

        if (student.IsAllowedToBorrow == allowBorrowing)
        {
            string state = allowBorrowing ? "allowed to borrow" : "restricted from borrowing";
            return (true, $"Student is already {state}. No changes were made.");
        }

        if (allowBorrowing)
        {
            student.RestoreBorrowingPrivilege();
        }
        else
        {
            student.SuspendBorrowing();
        }

        await _studentRepository.UpdateAsync(student, cancellationToken);
        return (true, $"Student eligibility updated: {(allowBorrowing ? "Eligible" : "Restricted")}.");
    }
}