using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed record ActiveBorrowingItem(
    int BorrowingId,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int EquipmentId,
    string EquipmentName,
    string AssetTag,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate)
{
    public string StudentDisplay => $"{StudentName} ({StudentNumber})";
    public string EquipmentDisplay => $"{EquipmentName} [{AssetTag}]";
    public string DateBorrowedDisplay => DateBorrowed.ToString("yyyy-MM-dd");
    public string ExpectedReturnDisplay => ExpectedReturnDate.ToString("yyyy-MM-dd");
}

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    public ObservableCollection<ActiveBorrowingItem> ActiveBorrowings { get; } = [];

    [ObservableProperty]
    private ActiveBorrowingItem? selectedBorrowing;

    [ObservableProperty]
    private DateTimeOffset? returnDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isSuccess;

    [ObservableProperty]
    private bool hasMessage;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
        _returnEquipmentService = returnEquipmentService;

        _ = LoadBorrowingsAsync();
    }

    [RelayCommand]
    public async Task LoadBorrowingsAsync()
    {
        int? previousSelectedId = SelectedBorrowing?.BorrowingId;
        ActiveBorrowings.Clear();

        IReadOnlyList<Borrowing> borrowings = await _borrowingRepository.GetActiveBorrowingsAsync();
        foreach (Borrowing b in borrowings)
        {
            Student? student = await _studentRepository.GetByIdAsync(b.StudentId);
            Equipment? equipment = await _equipmentRepository.GetByIdAsync(b.EquipmentId);

            var item = new ActiveBorrowingItem(
                BorrowingId: b.Id,
                StudentId: b.StudentId,
                StudentName: student?.FullName ?? $"Student #{b.StudentId}",
                StudentNumber: student?.StudentNumber ?? "N/A",
                EquipmentId: b.EquipmentId,
                EquipmentName: equipment?.Name ?? $"Equipment #{b.EquipmentId}",
                AssetTag: equipment?.AssetTag ?? "N/A",
                DateBorrowed: b.DateBorrowed,
                ExpectedReturnDate: b.ExpectedReturnDate);

            ActiveBorrowings.Add(item);
        }

        if (previousSelectedId.HasValue)
        {
            SelectedBorrowing = ActiveBorrowings.FirstOrDefault(b => b.BorrowingId == previousSelectedId.Value);
        }
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        HasMessage = false;
        StatusMessage = null;

        // Presentation validation (Part I)
        if (SelectedBorrowing is null)
        {
            SetMessage("Please select an active borrowing from the list to return.", false);
            return;
        }

        if (ReturnDate is null)
        {
            SetMessage("Please select a valid return date.", false);
            return;
        }

        DateOnly dateReturned = DateOnly.FromDateTime(ReturnDate.Value.DateTime);

        // Invoke application service (Part F)
        var request = new ReturnEquipmentRequest(SelectedBorrowing.BorrowingId, dateReturned);
        ReturnEquipmentResult result = await _returnEquipmentService.ReturnAsync(request);

        if (result.Succeeded)
        {
            SetMessage($"Success: {result.Message} '{SelectedBorrowing.EquipmentName}' marked returned.", true);
            SelectedBorrowing = null;
            await LoadBorrowingsAsync();
        }
        else
        {
            SetMessage($"Return Failed: {result.Message}", false);
        }
    }

    private void SetMessage(string message, bool success)
    {
        StatusMessage = message;
        IsSuccess = success;
        HasMessage = true;
    }
}
