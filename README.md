# Equipment Borrowing

Laboratory Activity 1: From Requirements to Application Structure

This project is my starting structure for a Campus Equipment Borrowing System. The goal is not to finish the full system yet, but to organize the code properly before adding a desktop UI or a database later.

## Part A: System Analysis

### Actors

| Actor | What the actor expects from the system |
| --- | --- |
| Student | Can request to borrow equipment if the item is available and the student is allowed to borrow. |
| Laboratory staff or system operator | Can rely on the system to record borrowings and update the equipment status when items are borrowed or returned. |

### Use Cases

| Use Case | Primary Actor | Preconditions | Main Action | Expected Result | Possible Failure |
| --- | --- | --- | --- | --- | --- |
| Borrow Equipment | Student | Student and equipment records exist. Equipment may be available. | Student requests to borrow a selected equipment item. | Borrowing is created and equipment becomes unavailable. | Student does not exist, student is not allowed, equipment does not exist, equipment is unavailable, or maximum active borrowings has been reached. |
| Return Equipment | Laboratory staff or system operator | Active borrowing record exists. | Returned item is recorded in the system. | Borrowing is marked returned and equipment becomes available again. | Borrowing does not exist or has already been returned. |
| Find Available Equipment | Student | Equipment records exist. | Student or operator searches for equipment that can be borrowed. | Available equipment is shown. | No equipment exists or all matching equipment is unavailable. |

### Domain Concepts

| Concept | Information contained | Rules or state | Not responsible for |
| --- | --- | --- | --- |
| Student | Id, student number, full name, borrowing privilege, maximum active borrowings | Keeps the student's borrowing status and allowed active borrowing limit. | Should not handle database access, UI actions, or borrowing approval by itself. |
| Equipment | Id, asset tag, name, availability | Can be marked borrowed or available. It should not be borrowed if it is already unavailable. | Should not decide whether a student is allowed to borrow. |
| Borrowing | Id, student id, equipment id, date borrowed, expected return date, status, date returned | Starts as active and can later be marked returned. The expected return date should not be earlier than the borrowed date. | Should not load student or equipment records from storage. |

## Solution Structure

```text
EquipmentBorrowing/
|-- EquipmentBorrowing.sln
|-- README.md
|-- src/
|   |-- EquipmentBorrowing.Domain/
|   |-- EquipmentBorrowing.Application/
|   |-- EquipmentBorrowing.Infrastructure/
|   `-- EquipmentBorrowing.Console/
`-- tests/
    `-- EquipmentBorrowing.Tests/
```

### Domain

`EquipmentBorrowing.Domain` contains the main business concepts: `Student`, `Equipment`, `Borrowing`, and `BorrowingStatus`. I placed the basic rules and state here because these things belong to the actual borrowing problem, not to the UI or database.

### Application

`EquipmentBorrowing.Application` contains the use-case logic. The main service here is `BorrowEquipmentService`, which checks the borrowing rules and uses repository interfaces to get or save data.

### Infrastructure

`EquipmentBorrowing.Infrastructure` contains technical implementations. For now, it uses in-memory repositories so the program can run without SQLite, Entity Framework Core, or another database.

### Tests

`EquipmentBorrowing.Tests` is the initial test project structure. I added a simple test-style method to show how the service could be checked without using a UI or database.

## Dependency Direction

```text
Executable / Future UI
          |
          v
    Application
          |
          v
       Domain

Infrastructure
          |
          v
Application interfaces and Domain
```

The domain project stays independent. The application project depends on the domain project. Infrastructure depends on the application interfaces and domain models because it provides the repository implementations. The console project connects the layers together only for demonstration.

## Use Case Mapping

Actor: Student

Use Case: Borrow Equipment

Application Service: `BorrowEquipmentService`

Domain Objects Used: `Student`, `Equipment`, `Borrowing`, `BorrowingStatus`

Repository Interfaces Used: `IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`

Infrastructure Implementations Used: `InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`

## Reflection

The application service should depend on repository interfaces instead of directly depending on a database implementation because the borrowing use case should not care where the data comes from. The same service should still make sense whether the data is stored in memory, SQLite, files, or another source.

If SQLite were added later, the domain models, repository interfaces, and `BorrowEquipmentService` could stay mostly the same. The main change would be adding SQLite repository classes inside the infrastructure project.

Avalonia views would eventually belong in a separate UI project, such as `EquipmentBorrowing.Desktop` or `EquipmentBorrowing.Avalonia`.

An Avalonia button should not directly execute database queries. The button should call an application service instead. This keeps the UI focused on user actions and leaves the business rules in the application layer.

The `BorrowEquipmentService.BorrowAsync` method represents the actual business operation requested by the actor. It checks the rules, creates the borrowing record, and updates equipment availability through the repository interfaces.

## Running the Demonstration

Install the .NET 8 SDK, then run:

```bash
dotnet build
dotnet run --project src/EquipmentBorrowing.Console
```

The console program demonstrates one successful borrowing request and failure cases for unavailable equipment and a student who is not allowed to borrow, as well as equipment return use cases.

---

# Laboratory Activity 2: Extending the Application with Avalonia UI and MVVM

This activity extends the Campus Equipment Borrowing System by building a presentation layer on top of the established architecture from Laboratory Activity 1, using Avalonia UI and the Model-View-ViewModel (MVVM) pattern with `CommunityToolkit.Mvvm`.

## 1. Desktop Project

### Responsibility of `EquipmentBorrowing.Desktop`
`EquipmentBorrowing.Desktop` acts as the presentation layer and the application composition root. Its primary responsibilities include:
- **Displaying information**: Rendering equipment inventory, student choices, active borrowings, and operational statuses using XAML layouts and data templates.
- **Collecting user input**: Receiving selections for borrowers, items, and loan dates through input controls like `ComboBox`, `ListBox`, and `DatePicker`.
- **Managing presentation state**: Tracking view-specific state (such as selected records, error messages, and view switching) inside ViewModels.
- **Invoking application operations**: Forwarding user intentions to the application layer via command bindings (`IAsyncRelayCommand`) that invoke application services.
- **Providing user-facing feedback**: Differentiating between presentation-level issues (e.g., missing input) and business-level outcomes (e.g., rule violations or successful approvals) with clear feedback banners.
- **Application Composition Root (`App.axaml.cs`)**: Centralizing dependency registration using `Microsoft.Extensions.DependencyInjection`, registering repositories as singletons to ensure in-memory state persists between views.

### Interaction with Existing Projects
- **References `EquipmentBorrowing.Application`**: ViewModels consume application services (`BorrowEquipmentService`, `ReturnEquipmentService`) and repository abstractions (`IEquipmentRepository`, `IStudentRepository`, `IBorrowingRepository`).
- **References `EquipmentBorrowing.Infrastructure`**: Referenced only at the composition root (`App.axaml.cs`) to wire concrete repository implementations (`InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`, `InMemoryStudentRepository`) into the DI container.
- **Indirectly depends on `EquipmentBorrowing.Domain`**: Uses domain models (`Equipment`, `Student`, `Borrowing`) passed to and from the application services, without altering domain logic.
- **Strict Boundary**: Neither `EquipmentBorrowing.Domain` nor `EquipmentBorrowing.Application` references Avalonia or the Desktop project.

## 2. Updated Architecture

```text
  Avalonia View (EquipmentView / BorrowingsView / MainWindow)
        |
        | Data Binding / Commands
        v
    ViewModel (EquipmentViewModel / BorrowingsViewModel / MainWindowViewModel)
        |
        | Application Operation (Requests / Results)
        v
Application Service (BorrowEquipmentService / ReturnEquipmentService)
        |
        +-----------------------------------> Domain (Student, Equipment, Borrowing)
        |
        v
Repository Interfaces (IStudentRepository, IEquipmentRepository, IBorrowingRepository)
        ^
        | Implemented by
        |
Infrastructure Implementation (InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository)
```

## 3. Borrow Equipment Flow

From the moment the user clicks the **"Borrow Equipment"** button in `EquipmentView`:

1. **User Interaction**: The user selects a student from the borrower dropdown, selects an equipment item from the list, chooses an expected return date, and clicks **Borrow Equipment**.
2. **View to ViewModel**: The button click triggers `BorrowCommand` on `EquipmentViewModel` via XAML command binding.
3. **Presentation Validation**: `EquipmentViewModel` checks that a student, equipment, and expected return date have been selected and that the return date is not earlier than today. If missing, it immediately sets an error message and stops without calling the application layer.
4. **Service Invocation**: `EquipmentViewModel` packages the user input into a `BorrowEquipmentRequest` and awaits `_borrowEquipmentService.BorrowAsync(request)`.
5. **Application Business Validation**:
   - The service fetches the student record from `IStudentRepository` and verifies the student exists and has borrowing privileges.
   - The service fetches the equipment record from `IEquipmentRepository` and verifies the item exists and is marked available.
   - The service queries `IBorrowingRepository` to check if the student has exceeded their active borrowing quota.
6. **Domain State Mutation**: If all rules pass, the service creates a new `Borrowing` domain entity and invokes `equipment.MarkBorrowed()`.
7. **Repository Persistence**: The service persists the new borrowing through `_borrowingRepository.AddAsync` and updates the equipment state through `_equipmentRepository.UpdateAsync`.
8. **Result Returned**: The service returns a `BorrowEquipmentResult` indicating success or failure.
9. **UI Refresh & User Feedback**: `EquipmentViewModel` inspects the result:
   - On success: Displays a confirmation banner, reloads the equipment collection from the repository (which now displays "Borrowed" in red), and clears transient inputs.
   - On failure: Displays the specific business rejection reason provided by the service in an alert banner.

## 4. Return Equipment Flow

From the moment the user clicks the **"Return Equipment"** button in `ActiveBorrowingsView`:

1. **User Interaction**: The user navigates to the "Active Borrowings" section, selects an active borrowing record from the list, confirms the return date, and clicks **Return Equipment**.
2. **View to ViewModel**: The button triggers `ReturnCommand` on `BorrowingsViewModel` via command binding.
3. **Presentation Validation**: `BorrowingsViewModel` verifies that a borrowing record is selected and a valid date is specified.
4. **Service Invocation**: `BorrowingsViewModel` constructs a `ReturnEquipmentRequest` containing the `BorrowingId` and `ReturnDate`, and awaits `_returnEquipmentService.ReturnAsync(request)`.
5. **Application Business Validation**:
   - `ReturnEquipmentService` retrieves the borrowing record from `IBorrowingRepository` and checks that it exists and has not already been returned (`Status == Active`).
   - The service retrieves the associated equipment record from `IEquipmentRepository`.
6. **Domain State Mutation**:
   - The borrowing entity transitions to returned status via `borrowing.MarkReturned(request.ReturnDate)`.
   - The equipment entity is marked available via `equipment.MarkAvailable()`.
7. **Repository Persistence**: The service persists the changes via `_borrowingRepository.UpdateAsync(borrowing)` and `_equipmentRepository.UpdateAsync(equipment)`.
8. **Result Returned**: The service returns `ReturnEquipmentResult.Success(...)` (or failure).
9. **UI Refresh & User Feedback**: `BorrowingsViewModel` displays a success confirmation and re-queries `_borrowingRepository.GetActiveBorrowingsAsync()`. The returned item immediately disappears from the active borrowings list. When navigating back to the Equipment Directory, the item's badge reflects "Available".

## 5. Architectural Reflection

### 1. Why should the View not call a repository directly?
Directly calling a repository from the View couples the graphical presentation directly to data retrieval logic, bypassing business rules and application validation. It also makes automated UI testing difficult and violates separation of concerns. The View's sole duty is presenting UI elements and capturing user inputs.

### 2. Why should business rules not be implemented in the ViewModel?
Business rules (such as checking active borrowing quotas, determining borrowing eligibility, or enforcing minimum return dates) define the domain problem and core business logic. If placed in the ViewModel, those rules cannot be reused across other interfaces (e.g., CLI tools, background workers, web APIs) and risk being duplicated or inconsistently applied.

### 3. What is the responsibility of the ViewModel?
The ViewModel acts as the bridge between the View and the Application Service. It maintains presentation state (e.g., selected items, loading states, input validation errors, and observable collections), translates user gestures into executable commands, delegates business operations to application services, and formats results into user-facing messages.

### 4. Why can the existing Application layer work without knowing that Avalonia is being used?
The Application layer depends solely on Domain entities and repository abstractions (`IEquipmentRepository`, etc.). It communicates using standard C# types, records, and tasks. Because it has no references to Avalonia, XAML, or any presentation framework, it is completely decoupled and unaware of whether its caller is Avalonia, WPF, a console app, or a web service.

### 5. What advantage is gained from registering dependencies in one composition point?
Registering dependencies in a single composition root (`App.axaml.cs`) provides centralized configuration, simplifies lifetime management (such as registering singleton in-memory repositories so state persists across views), and enables loose coupling throughout the application. It also makes swapping out components (e.g., changing in-memory repositories to database repositories) trivial without modifying the consumer classes.

### 6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?
The entire UI layer (`EquipmentView`, `BorrowingsView`, `MainWindow`), all ViewModels (`EquipmentViewModel`, `BorrowingsViewModel`, `MainWindowViewModel`), and all Application Services (`BorrowEquipmentService`, `ReturnEquipmentService`) would remain completely unchanged. Only the Infrastructure project would introduce SQLite-backed implementations of `IEquipmentRepository`, `IStudentRepository`, and `IBorrowingRepository`, and `App.axaml.cs` would register those new implementations.

## Running the Avalonia Desktop Application

To launch the desktop interface:

```bash
dotnet run --project src/EquipmentBorrowing.Desktop
```

---

# Laboratory Activity 3: From In-Memory Data to Persistent Storage

This activity extends the Campus Equipment Borrowing System by introducing persistent relational storage using **SQLite** and **Entity Framework Core (EF Core)**, replacing the temporary in-memory repositories while preserving the established clean architecture.

## 1. Relational Database Design

### Database Diagram

```text
+-----------------------------------+          +--------------------------------------+
|             STUDENTS              |          |              EQUIPMENT               |
+-----------------------------------+          +--------------------------------------+
| Id (PK, INTEGER, AUTOINCREMENT)   |          | Id (PK, INTEGER, AUTOINCREMENT)      |
| StudentNumber (TEXT, UNIQUE, REQ) |          | AssetTag (TEXT, UNIQUE, REQ)         |
| FullName (TEXT, REQ)              |          | Name (TEXT, REQ)                     |
| IsAllowedToBorrow (INTEGER, REQ)  |          | IsAvailable (INTEGER, REQ)           |
| MaximumActiveBorrowings (INT, REQ)|          +--------------------------------------+
+-----------------------------------+                             | 1
                  | 1                                             |
                  |                                               |
                  |               +-------------------------------+
                  |               |
                  | *             | *
+----------------------------------------------------+
|                    BORROWINGS                      |
+----------------------------------------------------+
| Id (PK, INTEGER, AUTOINCREMENT)                    |
| StudentId (FK -> Students.Id, INTEGER, REQ)        |
| EquipmentId (FK -> Equipment.Id, INTEGER, REQ)     |
| DateBorrowed (TEXT, REQ)                           |
| ExpectedReturnDate (TEXT, REQ)                     |
| Status (INTEGER, REQ)                              |
| DateReturned (TEXT, NULLABLE)                      |
+----------------------------------------------------+
```

### Table Definitions & Constraints
- **Students Table**:
  - `Id`: Primary Key, auto-incrementing integer.
  - `StudentNumber`: Unique business identifier, required, max length 20.
  - `FullName`: Required string, max length 100.
  - `IsAllowedToBorrow`: Required boolean flag.
  - `MaximumActiveBorrowings`: Required integer limit.
- **Equipment Table**:
  - `Id`: Primary Key, auto-incrementing integer.
  - `AssetTag`: Unique barcode/asset identifier, required, max length 30.
  - `Name`: Required descriptive name, max length 100.
  - `IsAvailable`: Required boolean availability state.
- **Borrowings Table**:
  - `Id`: Primary Key, auto-incrementing integer.
  - `StudentId`: Foreign Key referencing `Students(Id)`.
  - `EquipmentId`: Foreign Key referencing `Equipment(Id)`.
  - `DateBorrowed` & `ExpectedReturnDate`: Required `DateOnly` ISO format strings.
  - `Status`: Integer enum (`0 = Active`, `1 = Returned`).
  - `DateReturned`: Nullable string.
  - Index on `(StudentId, Status)` for fast active-borrowing lookups.

---

## 2. SQLite and EF Core Integration

EF Core was added to the `EquipmentBorrowing.Infrastructure` project via NuGet:
- `Microsoft.EntityFrameworkCore.Sqlite`: The SQLite database provider.
- `Microsoft.EntityFrameworkCore.Design`: Provides design-time tooling for generating and executing migrations.

The database file `equipment_borrowing.db` is stored locally in the application's runtime folder.

---

## 3. DbContext Responsibility

`EquipmentBorrowingDbContext` is the central gateway to the database. Its responsibilities are:
- Exposing `DbSet<Student>`, `DbSet<Equipment>`, and `DbSet<Borrowing>` collections.
- Mapping Domain classes to relational tables using entity configurations (`IEntityTypeConfiguration<T>`).
- Managing database connections, transactions, and tracking entity states.
- Translating LINQ queries into optimized SQLite SQL statements.

---

## 4. Repository Transition

The repository layer was transitioned seamlessly:

```text
Repository Interface (Application Layer)
       │
       ▼
Previously: InMemoryRepository  ──► List<T> in RAM (lost on application exit)
Now:        EfRepository        ──► DbContext ──► SQLite (persisted to disk)
```

Because `BorrowEquipmentService`, `ReturnEquipmentService`, and all ViewModels depend exclusively on `IStudentRepository`, `IEquipmentRepository`, and `IBorrowingRepository`, **not a single line of business or UI code needed to be rewritten**.

---

## 5. Migration Process

Database migrations were created and applied using the .NET CLI:

1. **Create the Migration**:
   ```bash
   dotnet ef migrations add InitialCreate --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Desktop
   ```
2. **Apply the Migration to SQLite**:
   ```bash
   dotnet ef database update --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Desktop
   ```

---

## 6. Generated SQL Inspection

### Example 1: Active Borrowings Query
- **LINQ Query:**
  ```csharp
  var active = await _context.Borrowings
      .AsNoTracking()
      .Where(b => b.Status == BorrowingStatus.Active)
      .ToListAsync();
  ```
- **Generated SQL:**
  ```sql
  SELECT "b"."Id", "b"."DateBorrowed", "b"."DateReturned", "b"."EquipmentId", "b"."ExpectedReturnDate", "b"."Status", "b"."StudentId"
  FROM "Borrowings" AS "b"
  WHERE "b"."Status" = 0;
  ```
- **Explanation:** EF Core translates the LINQ `.Where()` expression into an SQL `WHERE "b"."Status" = 0` clause, retrieving all records matching the `Active` enum value.

### Example 2: Count Active Borrowings by Student
- **LINQ Query:**
  ```csharp
  int count = await _context.Borrowings
      .AsNoTracking()
      .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
  ```
- **Generated SQL:**
  ```sql
  SELECT COUNT(*)
  FROM "Borrowings" AS "b"
  WHERE "b"."StudentId" = @__studentId_0 AND "b"."Status" = 0;
  ```
- **Explanation:** EF Core executes a parameter-safe aggregate query directly in the database engine, returning only a scalar integer count instead of loading entities into memory.

---

## 7. Tracking vs. No-Tracking Queries

- **Read-Only Queries (`AsNoTracking()`):** Used in `GetAllAsync()` and `GetActiveBorrowingsAsync()`. Since items are displayed on the UI without immediate direct in-context mutation, disabling change tracking saves memory and significantly speeds up query execution.
- **Tracking Queries:** Used in `GetByIdAsync()`. When borrowing or returning equipment, the entities must be fetched with tracking enabled so EF Core detects status changes (`MarkBorrowed()`, `MarkReturned()`) when `SaveChangesAsync()` is called.

---

## 8. Persistence Demonstration

Persistence was verified through the following test:
1. Launched the application and borrowed a piece of equipment.
2. Verified that the equipment marked as "Borrowed" and appeared in "Active Borrowings".
3. Closed the application completely.
4. Re-launched the application.
5. Observed that the equipment remained marked as "Borrowed" and the active borrowing record persisted from the SQLite database.
6. Processed a return, restarted the application again, and confirmed the returned state remained stored.

---

## 9. Architectural Reflection

1. **Why did the application not need to be completely rewritten when SQLite was introduced?**
   Because the application follows the Dependency Inversion Principle. The application services and ViewModels depend on repository abstractions (`interfaces`), not concrete storage classes. Replacing in-memory storage with SQLite only required writing new repository implementations behind those same interfaces.

2. **Why should the ViewModel not use DbContext directly?**
   Using `DbContext` inside ViewModels tightly couples the user interface directly to the database. This would bypass application business logic and validation, break unit testing, and leak database concerns into the presentation layer.

3. **What responsibility does the repository implementation now perform?**
   The repository mediates between the domain/application layer and the data mapping layer. It translates high-level domain operations into EF Core LINQ queries, coordinates change saving, and hides EF Core and SQLite details from the rest of the application.

4. **What is the purpose of an EF Core migration?**
   An EF Core migration keeps the database schema synchronized with the C# model classes in a version-controlled, repeatable manner without manually writing DDL scripts.

5. **Why are foreign keys important in the borrowing database?**
   Foreign keys enforce referential integrity. They guarantee that every borrowing record points to a real student and a real equipment item, preventing orphan records or invalid loan transactions.

6. **Why can a read-only query benefit from `AsNoTracking()`?**
   `AsNoTracking()` informs EF Core not to store snapshot copies of entity instances in the change tracker. This reduces memory allocation and eliminates unnecessary change detection overhead.

7. **What would happen to the rest of the application if the SQLite implementation were replaced later by another database provider?**
   Only the configuration in the Infrastructure layer and the connection string in the composition root (`App.axaml.cs`) would change (e.g., swapping `.UseSqlite()` for `.UseNpgsql()` or `.UseSqlServer()`). All domain models, interfaces, application services, ViewModels, and Avalonia Views would remain completely untouched.


