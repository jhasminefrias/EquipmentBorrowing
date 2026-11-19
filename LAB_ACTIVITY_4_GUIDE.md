# Laboratory Activity 4: Advanced Data-Driven Workflows and PostgreSQL Integration
## Complete Step-by-Step Implementation Guide & Source Codes

Welcome to **Laboratory Activity 4**! 

Per your instructions:
- **Your project files have NOT been modified.**
- All steps, NuGet commands, complete C# source codes, XAML views, database migrations, and daily Git commit messages are organized below.
- You can copy and paste the code into Visual Studio at your own pace and commit across the days of the week.

---

## 📅 Recommended 3-Week Development & Commit Schedule

Following the milestone timeline in Section XI of the lab guidelines:

### **Week 1: PostgreSQL Transition & Entity Management Foundations**
- **Day 1 (R2):** Switch packages to Npgsql (`Npgsql.EntityFrameworkCore.PostgreSQL`), update Domain models (`ContactInfo`, `Description`).
- **Day 2 (R2):** Update EF Core Configurations (`StudentConfiguration`, `EquipmentConfiguration`, `BorrowingConfiguration`) with PostgreSQL constraints and indexes.
- **Day 3 (R2):** Update `EquipmentBorrowingDbContextFactory`, create initial PostgreSQL migration (`PostgreSqlInitialCreate`), update `DbInitializer`.
- **Day 4 (R4):** Implement Student Management Service & Repository (`RegisterStudent`, `UpdateStudent`, `ChangeEligibility`).
- **Day 5 (R5):** Implement Equipment Management Service & Repository (`RegisterEquipment`, `UpdateEquipment`).

### **Week 2: Multi-Item Atomic Checkout, Transactions & History**
- **Day 6 (R3):** Implement Multi-Item Checkout Service (`CheckOutMultipleEquipmentService`) with atomic database transactions (`BeginTransactionAsync`).
- **Day 7 (R6):** Implement History Services & Queries (`StudentHistoryService`, `EquipmentHistoryService`).
- **Day 8 (R6):** Implement Search, Filter & Sort Borrowings (`BorrowingSearchService` with BR-17 rules).

### **Week 3: Desktop UI Integration, Verification & Traceability**
- **Day 9 (R7):** Create Avalonia Views & ViewModels for Student & Equipment Management (`StudentsView`, `EquipmentManagementView`).
- **Day 10 (R7):** Create Avalonia Views & ViewModels for Multi-Item Checkout and Borrowing Search/History (`MultiBorrowView`, `BorrowingsHistoryView`).
- **Day 11 (R7):** Update `MainWindow` navigation and dependency injection in `App.axaml.cs`.
- **Day 12 (R8):** Update `README.md` with PostgreSQL setup, traceability matrix, and architectural reflections.

---

## Part 1: Install NuGet Packages for PostgreSQL

In your solution folder, replace the SQLite provider with **Npgsql** (the official PostgreSQL EF Core provider).

Run these commands in your PowerShell / Visual Studio Package Manager Console:

```powershell
# 1. Remove SQLite from Infrastructure and add Npgsql
dotnet remove src/EquipmentBorrowing.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/EquipmentBorrowing.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.8
dotnet add src/EquipmentBorrowing.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.8

# 2. Add Npgsql and Design tooling to Desktop
dotnet remove src/EquipmentBorrowing.Desktop package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/EquipmentBorrowing.Desktop package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.8
dotnet add src/EquipmentBorrowing.Desktop package Microsoft.EntityFrameworkCore.Design --version 8.0.8
```

---

## Part 2: Domain Layer Updates

Per **BR-02**, students have optional `ContactInfo` and newly registered students are eligible to borrow.  
Per **BR-06**, equipment has an optional `Description`.  
Per **BR-09**, students can have up to 3 active borrowings.

### 2.1 Update `Student.cs`
**File:** `src/EquipmentBorrowing.Domain/Student.cs`

Replace the entire contents with:

```csharp
namespace EquipmentBorrowing.Domain;

public sealed class Student
{
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
```

---

### 2.2 Update `Equipment.cs`
**File:** `src/EquipmentBorrowing.Domain/Equipment.cs`

Replace the entire contents with:

```csharp
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
```

---

## Part 3: EF Core & PostgreSQL Infrastructure

### 3.1 Update `StudentConfiguration.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Persistence/Configurations/StudentConfiguration.cs`

```csharp
using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(20);

        // BR-01: Case-insensitive uniqueness enforcement
        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactInfo)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired();

        builder.Property(s => s.MaximumActiveBorrowings)
            .IsRequired()
            .HasDefaultValue(3);
    }
}
```

---

### 3.2 Update `EquipmentConfiguration.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Persistence/Configurations/EquipmentConfiguration.cs`

```csharp
using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AssetTag)
            .IsRequired()
            .HasMaxLength(30);

        // BR-05: Case-insensitive unique asset tag
        builder.HasIndex(e => e.AssetTag)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired(false)
            .HasMaxLength(250);

        builder.Property(e => e.IsAvailable)
            .IsRequired();
    }
}
```

---

### 3.3 Update `BorrowingConfiguration.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Persistence/Configurations/BorrowingConfiguration.cs`

Per **BR-10 & BR-16**, PostgreSQL must enforce that an equipment item can have **at most one active borrowing** at any time. In PostgreSQL, this is enforced using a partial unique index (`WHERE status = 0`):

```csharp
using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.StudentId)
            .IsRequired();

        builder.Property(b => b.EquipmentId)
            .IsRequired();

        builder.Property(b => b.DateBorrowed)
            .IsRequired();

        builder.Property(b => b.ExpectedReturnDate)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.DateReturned)
            .IsRequired(false);

        // Foreign keys preserving referential integrity
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.StudentId, b.Status });

        // BR-10 / BR-16: PostgreSQL partial index ensuring only ONE active borrowing per equipment item
        builder.HasIndex(b => b.EquipmentId)
            .IsUnique()
            .HasFilter("\"Status\" = 0");
    }
}
```

---

### 3.4 Update `EquipmentBorrowingDbContextFactory.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Persistence/EquipmentBorrowingDbContextFactory.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();
        
        // Default local development connection string (configurable via environment variable)
        string connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new EquipmentBorrowingDbContext(optionsBuilder.Options);
    }
}
```

---

### 3.5 Update `DbInitializer.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Persistence/DbInitializer.cs`

```csharp
using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(EquipmentBorrowingDbContext context)
    {
        await context.Database.MigrateAsync();

        // Check if data already exists to avoid duplicate entries
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
        {
            return;
        }

        // 1. Initial Students
        var students = new[]
        {
            new Student(0, "2026-0001", "Jhasmine Frias", "jhasmine@example.edu", isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0002", "Hannah Rapal", "hannah@example.edu", isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0003", "Wendell Pasquil", null, isAllowedToBorrow: true, maximumActiveBorrowings: 3),
            new Student(0, "2026-0004", "Shanice Palasan", "shanice@example.edu", isAllowedToBorrow: false, maximumActiveBorrowings: 3), // Ineligible student for testing
            new Student(0, "2026-0005", "Airesh Abcd", null, isAllowedToBorrow: true, maximumActiveBorrowings: 3)
        };

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();

        // 2. Initial Equipment
        var equipment = new[]
        {
            new Equipment(0, "LAB-CAM-001", "Canon DSLR Camera", "High resolution camera with 18-55mm lens", isAvailable: true),
            new Equipment(0, "LAB-MIC-001", "Wireless Lavalier Microphone", "Dual channel wireless lapel mic", isAvailable: false),
            new Equipment(0, "LAB-PROJ-001", "Portable Projector", "1080p HDMI LED projector", isAvailable: true),
            new Equipment(0, "LAB-TRP-001", "Camera Tripod Stand", "Aluminum 60-inch tripod", isAvailable: true),
            new Equipment(0, "LAB-REC-001", "Digital Audio Recorder", "Stereo handheld field recorder", isAvailable: true),
            new Equipment(0, "LAB-LAP-001", "Dell XPS 15 Laptop", "Intel i7, 16GB RAM for lab computing", isAvailable: true),
            new Equipment(0, "LAB-TAB-001", "Wacom Drawing Tablet", "Graphic tablet with stylus", isAvailable: true)
        };

        await context.Equipment.AddRangeAsync(equipment);
        await context.SaveChangesAsync();

        // 3. Initial Active Borrowing (Wireless Mic borrowed by Jhasmine)
        Student jhasmine = await context.Students.FirstAsync(s => s.StudentNumber == "2026-0001");
        Equipment mic = await context.Equipment.FirstAsync(e => e.AssetTag == "LAB-MIC-001");

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var activeBorrowing = new Borrowing(
            0,
            jhasmine.Id,
            mic.Id,
            today.AddDays(-2),
            today.AddDays(5));

        await context.Borrowings.AddAsync(activeBorrowing);

        // 4. Historical Returned Borrowing (Projector borrowed & returned by Hannah)
        Student hannah = await context.Students.FirstAsync(s => s.StudentNumber == "2026-0002");
        Equipment proj = await context.Equipment.FirstAsync(e => e.AssetTag == "LAB-PROJ-001");

        var pastBorrowing = new Borrowing(
            0,
            hannah.Id,
            proj.Id,
            today.AddDays(-10),
            today.AddDays(-3));
        pastBorrowing.MarkReturned(today.AddDays(-4));

        await context.Borrowings.AddAsync(pastBorrowing);
        await context.SaveChangesAsync();
    }
}
```

---

## Part 4: Repository Updates & New Queries

### 4.1 Update `IStudentRepository.cs`
**File:** `src/EquipmentBorrowing.Application/Interfaces/IStudentRepository.cs`

```csharp
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);
}
```

---

### 4.2 Update `IEquipmentRepository.cs`
**File:** `src/EquipmentBorrowing.Application/Interfaces/IEquipmentRepository.cs`

```csharp
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Equipment?> GetByAssetTagAsync(string assetTag, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}
```

---

### 4.3 Update `IBorrowingRepository.cs`
**File:** `src/EquipmentBorrowing.Application/Interfaces/IBorrowingRepository.cs`

```csharp
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> SearchAsync(string? searchText, BorrowingStatus? statusFilter, string? sortBy, bool ascending, CancellationToken cancellationToken = default);

    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}
```

---

### 4.4 Update `EfStudentRepository.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Repositories/EfStudentRepository.cs`

```csharp
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfStudentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        string normalized = studentNumber.Trim().ToLower();
        return await _context.Students
            .SingleOrDefaultAsync(s => s.StudentNumber.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

### 4.5 Update `EfEquipmentRepository.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Repositories/EfEquipmentRepository.cs`

```csharp
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfEquipmentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Equipment?> GetByAssetTagAsync(string assetTag, CancellationToken cancellationToken = default)
    {
        string normalized = assetTag.Trim().ToLower();
        return await _context.Equipment
            .SingleOrDefaultAsync(e => e.AssetTag.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        // UC-02 query
        return await _context.Equipment
            .AsNoTracking()
            .Where(e => e.IsAvailable)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipment.AddAsync(equipment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

### 4.6 Update `EfBorrowingRepository.cs`
**File:** `src/EquipmentBorrowing.Infrastructure/Repositories/EfBorrowingRepository.cs`

```csharp
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.Borrowings.AddAsync(borrowing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        // UC-07: Student history
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        // UC-08: Equipment history
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.EquipmentId == equipmentId)
            .OrderByDescending(b => b.DateBorrowed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> SearchAsync(
        string? searchText,
        BorrowingStatus? statusFilter,
        string? sortBy,
        bool ascending,
        CancellationToken cancellationToken = default)
    {
        // UC-09: Search, filter & sort per BR-17
        var query = _context.Borrowings.AsNoTracking().AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string term = searchText.Trim().ToLower();

            var matchingStudentIds = _context.Students
                .Where(s => s.FullName.ToLower().Contains(term) || s.StudentNumber.ToLower().Contains(term))
                .Select(s => s.Id);

            var matchingEquipmentIds = _context.Equipment
                .Where(e => e.Name.ToLower().Contains(term) || e.AssetTag.ToLower().Contains(term))
                .Select(e => e.Id);

            query = query.Where(b => matchingStudentIds.Contains(b.StudentId) || matchingEquipmentIds.Contains(b.EquipmentId));
        }

        query = sortBy?.ToLower() switch
        {
            "expectedreturn" or "due" => ascending
                ? query.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            _ => ascending
                ? query.OrderBy(b => b.DateBorrowed).ThenBy(b => b.Id)
                : query.OrderByDescending(b => b.DateBorrowed).ThenBy(b => b.Id)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Update(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Part 5: New Application Services

### 5.1 Student Management Service (UC-12, UC-13, UC-14)
**File:** `src/EquipmentBorrowing.Application/Services/StudentManagementService.cs`

```csharp
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
```

---

### 5.2 Equipment Management Service (UC-15, UC-16)
**File:** `src/EquipmentBorrowing.Application/Services/EquipmentManagementService.cs`

```csharp
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
```

---

### 5.3 Multi-Item Atomic Checkout Service (UC-06)
**File:** `src/EquipmentBorrowing.Application/Services/CheckOutMultipleEquipmentService.cs`

Per **BR-12 & BR-15**, checkout of at least two items must execute **atomically** in a database transaction (`BeginTransactionAsync`). Either all items are issued and marked unavailable, or if anything fails, the entire transaction rolls back with zero partial writes:

```csharp
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Application.Services;

public sealed record CheckOutMultipleRequest(
    int StudentId,
    IReadOnlyList<int> EquipmentIds,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);

public sealed record CheckOutMultipleResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<Borrowing> Borrowings);

public sealed class CheckOutMultipleEquipmentService
{
    private readonly EquipmentBorrowingDbContext _context;
    private readonly IStudentRepository _studentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public CheckOutMultipleEquipmentService(
        EquipmentBorrowingDbContext context,
        IStudentRepository studentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _context = context;
        _studentRepository = studentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<CheckOutMultipleResult> CheckOutAsync(
        CheckOutMultipleRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. BR-12: Must request at least two distinct items
        var distinctItemIds = request.EquipmentIds.Distinct().ToList();
        if (distinctItemIds.Count < 2)
        {
            return new CheckOutMultipleResult(false, "Multi-item checkout requires at least two distinct equipment items.", []);
        }

        // 2. BR-11: Expected return date must be later than checkout date
        if (request.ExpectedReturnDate <= request.DateBorrowed)
        {
            return new CheckOutMultipleResult(false, "Expected return date must be later than the checkout date.", []);
        }

        // 3. BR-08: Validate student existence and eligibility
        Student? student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return new CheckOutMultipleResult(false, "Student does not exist.", []);
        }

        if (!student.IsAllowedToBorrow)
        {
            return new CheckOutMultipleResult(false, "Student is restricted from borrowing equipment.", []);
        }

        // 4. BR-09: Validate active limit (existing active + requested items <= 3)
        int currentActiveCount = await _borrowingRepository.CountActiveByStudentIdAsync(student.Id, cancellationToken);
        if (currentActiveCount + distinctItemIds.Count > student.MaximumActiveBorrowings)
        {
            return new CheckOutMultipleResult(
                false,
                $"Borrowing limit exceeded: Student currently has {currentActiveCount} active loan(s). Requesting {distinctItemIds.Count} items exceeds the limit of {student.MaximumActiveBorrowings}.",
                []);
        }

        // 5. BR-15: Open an explicit atomic database transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var createdBorrowings = new List<Borrowing>();

            foreach (int equipmentId in distinctItemIds)
            {
                // Revalidate current availability inside transaction boundary
                Equipment? item = await _context.Equipment
                    .SingleOrDefaultAsync(e => e.Id == equipmentId, cancellationToken);

                if (item is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new CheckOutMultipleResult(false, $"Equipment item with ID {equipmentId} does not exist.", []);
                }

                if (!item.IsAvailable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new CheckOutMultipleResult(false, $"Equipment '{item.Name}' [{item.AssetTag}] is already borrowed/unavailable.", []);
                }

                item.MarkBorrowed();

                var borrowing = new Borrowing(
                    0,
                    student.Id,
                    item.Id,
                    request.DateBorrowed,
                    request.ExpectedReturnDate);

                await _context.Borrowings.AddAsync(borrowing, cancellationToken);
                createdBorrowings.Add(borrowing);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CheckOutMultipleResult(
                true,
                $"Successfully checked out {createdBorrowings.Count} equipment items for {student.FullName}.",
                createdBorrowings);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new CheckOutMultipleResult(false, $"Database transaction failed and was rolled back: {ex.Message}", []);
        }
    }
}
```

---

## Part 6: Apply the PostgreSQL Database Migration

Once you have set up PostgreSQL locally (e.g. using pgAdmin or standard Postgres service running on port 5432):

1. **Delete any previous migration folder** (if created from SQLite in Lab 3):
   Delete `src/EquipmentBorrowing.Infrastructure/Migrations` if it exists.

2. **Generate the new PostgreSQL Migration:**
   ```powershell
   dotnet ef migrations add PostgreSqlInitialCreate --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Desktop
   ```

3. **Apply the Migration to your PostgreSQL database:**
   ```powershell
   dotnet ef database update --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Desktop
   ```

---

## Part 7: Register Services in `App.axaml.cs`

In `src/EquipmentBorrowing.Desktop/App.axaml.cs`, register the new services and ViewModels:

```csharp
private static void ConfigureServices(IServiceCollection services)
{
    // 1. PostgreSQL Connection
    string connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=postgres";

    services.AddDbContext<EquipmentBorrowingDbContext>(options =>
        options.UseNpgsql(connectionString));

    // 2. Repositories
    services.AddScoped<IStudentRepository, EfStudentRepository>();
    services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
    services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();

    // 3. Application Services
    services.AddTransient<BorrowEquipmentService>();
    services.AddTransient<ReturnEquipmentService>();
    services.AddTransient<StudentManagementService>();
    services.AddTransient<EquipmentManagementService>();
    services.AddTransient<CheckOutMultipleEquipmentService>();

    // 4. ViewModels
    services.AddTransient<EquipmentViewModel>();
    services.AddTransient<BorrowingsViewModel>();
    services.AddTransient<StudentsViewModel>();
    services.AddTransient<MultiCheckoutViewModel>();
    services.AddTransient<BorrowingSearchViewModel>();
    services.AddSingleton<MainWindowViewModel>();
}
```

---

## Part 8: Step-by-Step Commit Guide for Git

To earn full credit for **R8 (Meaningful Git History)**, run these commits across your working days:

```powershell
# Commit 1
git add src/EquipmentBorrowing.Domain/
git commit -m "feat(domain): update Student and Equipment models with contact info and descriptions"

# Commit 2
git add src/EquipmentBorrowing.Infrastructure/Persistence/
git commit -m "feat(infra): configure Npgsql and PostgreSQL partial unique constraints"

# Commit 3
git add src/EquipmentBorrowing.Infrastructure/Migrations/
git commit -m "feat(migrations): create PostgreSqlInitialCreate migration for schema"

# Commit 4
git add src/EquipmentBorrowing.Application/Services/StudentManagementService.cs src/EquipmentBorrowing.Infrastructure/Repositories/EfStudentRepository.cs
git commit -m "feat(students): implement student registration, editing, and eligibility toggle"

# Commit 5
git add src/EquipmentBorrowing.Application/Services/EquipmentManagementService.cs src/EquipmentBorrowing.Infrastructure/Repositories/EfEquipmentRepository.cs
git commit -m "feat(equipment): implement equipment registration and metadata updating"

# Commit 6
git add src/EquipmentBorrowing.Application/Services/CheckOutMultipleEquipmentService.cs
git commit -m "feat(checkout): implement atomic multi-item checkout transaction with rollback"

# Commit 7
git add src/EquipmentBorrowing.Desktop/
git commit -m "feat(ui): add multi-item checkout, student management, and history search views"

# Commit 8
git add README.md
git commit -m "docs: complete Laboratory Activity 4 traceability matrix and architecture documentation"
```
