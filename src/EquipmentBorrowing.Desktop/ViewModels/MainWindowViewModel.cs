using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly EquipmentViewModel _equipmentViewModel;

    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    public MainWindowViewModel(EquipmentViewModel equipmentViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        // BorrowingsViewModel comes with Part F (Return Equipment) —
        // placeholder for now so the button doesn't break navigation.
        CurrentViewModel = null;
    }
}