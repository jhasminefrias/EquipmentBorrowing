using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;

        string name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}