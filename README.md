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

