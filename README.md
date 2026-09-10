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

The console program demonstrates one successful borrowing request and failure cases for unavailable equipment and a student who is not allowed to borrow.
