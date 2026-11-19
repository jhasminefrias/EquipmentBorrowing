using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;
    private int _nextId;

    public InMemoryStudentRepository(IEnumerable<Student> students)
    {
        _students = students.ToList();
        _nextId = _students.Count > 0 ? _students.Max(s => s.Id) + 1 : 1;
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Student? student = _students.SingleOrDefault(s => s.Id == id);
        return Task.FromResult(student);
    }

    public Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        string normalized = studentNumber.Trim().ToLower();
        Student? student = _students.SingleOrDefault(s => s.StudentNumber.ToLower() == normalized);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Student> list = _students.OrderBy(s => s.FullName).ToList();
        return Task.FromResult(list);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students.Add(student);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        int index = _students.FindIndex(s => s.Id == student.Id);
        if (index >= 0)
        {
            _students[index] = student;
        }
        return Task.CompletedTask;
    }
}