using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student> students)
    {
        _students = students.ToList();
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Student? student = _students.SingleOrDefault(student => student.Id == id);
        return Task.FromResult(student);
    }
}
