using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ViewModelBase
{
    private readonly IEquipmentRepository _equipmentRepository;

    public ObservableCollection<Equipment> EquipmentItems { get; } = [];

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private string? statusMessage;

    public EquipmentViewModel(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
        _ = LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task LoadEquipmentAsync()
    {
        EquipmentItems.Clear();

        IReadOnlyList<Equipment> items = await _equipmentRepository.GetAllAsync();
        foreach (Equipment item in items)
        {
            EquipmentItems.Add(item);
        }
    }
}