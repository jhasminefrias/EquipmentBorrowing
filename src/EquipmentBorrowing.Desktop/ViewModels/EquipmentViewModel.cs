using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    public ObservableCollection<Equipment> EquipmentItems { get; } = [];
    public ObservableCollection<Student> Students { get; } = [];

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private DateTimeOffset? expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isSuccess;

    [ObservableProperty]
    private bool hasMessage;

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
        _borrowEquipmentService = borrowEquipmentService;

        _ = RefreshAllAsync();
    }

    [RelayCommand]
    public async Task RefreshAllAsync()
    {
        await LoadEquipmentAsync();
        await LoadStudentsAsync();
    }

    [RelayCommand]
    public async Task LoadEquipmentAsync()
    {
        int? previousSelectedId = SelectedEquipment?.Id;
        EquipmentItems.Clear();

        IReadOnlyList<Equipment> items = await _equipmentRepository.GetAllAsync();
        foreach (Equipment item in items)
        {
            EquipmentItems.Add(item);
        }

        if (previousSelectedId.HasValue)
        {
            SelectedEquipment = EquipmentItems.FirstOrDefault(e => e.Id == previousSelectedId.Value);
        }
    }

    [RelayCommand]
    public async Task LoadStudentsAsync()
    {
        int? previousSelectedId = SelectedStudent?.Id;
        Students.Clear();

        IReadOnlyList<Student> students = await _studentRepository.GetAllAsync();
        foreach (Student student in students)
        {
            Students.Add(student);
        }

        if (previousSelectedId.HasValue)
        {
            SelectedStudent = Students.FirstOrDefault(s => s.Id == previousSelectedId.Value);
        }
        else if (Students.Count > 0)
        {
            SelectedStudent = Students[0];
        }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        HasMessage = false;
        StatusMessage = null;

        // Presentation validation (Part I)
        if (SelectedStudent is null)
        {
            SetMessage("Please select a student.", false);
            return;
        }

        if (SelectedEquipment is null)
        {
            SetMessage("Please select an equipment item to borrow.", false);
            return;
        }

        if (ExpectedReturnDate is null)
        {
            SetMessage("Please select an expected return date.", false);
            return;
        }

        DateOnly dateBorrowed = DateOnly.FromDateTime(DateTime.Today);
        DateOnly expectedReturn = DateOnly.FromDateTime(ExpectedReturnDate.Value.DateTime);

        if (expectedReturn < dateBorrowed)
        {
            SetMessage("Expected return date cannot be earlier than today.", false);
            return;
        }

        // Invoke application service (Part E)
        var request = new BorrowEquipmentRequest(
            SelectedStudent.Id,
            SelectedEquipment.Id,
            dateBorrowed,
            expectedReturn);

        BorrowEquipmentResult result = await _borrowEquipmentService.BorrowAsync(request);

        if (result.Succeeded)
        {
            SetMessage($"Success: {result.Message} Equipment '{SelectedEquipment.Name}' borrowed by {SelectedStudent.FullName}.", true);
            await LoadEquipmentAsync();
        }
        else
        {
            SetMessage($"Borrowing Failed: {result.Message}", false);
        }
    }

    private void SetMessage(string message, bool success)
    {
        StatusMessage = message;
        IsSuccess = success;
        HasMessage = true;
    }
}