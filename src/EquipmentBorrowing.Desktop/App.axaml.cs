using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Seed data for the in-memory repositories. Registered as singletons
        // so state (e.g. an equipment item becoming unavailable) survives
        // navigation between views instead of resetting every time.
        var students = new[]
        {
            new Student(1, "2026-0001", "Alex Reyes", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(2, "2026-0002", "Jamie Santos", isAllowedToBorrow: false, maximumActiveBorrowings: 2)
        };

        var equipment = new[]
        {
            new Equipment(1, "LAB-CAM-001", "Digital Camera", isAvailable: true),
            new Equipment(2, "LAB-MIC-001", "Wireless Microphone", isAvailable: false),
            new Equipment(3, "LAB-PROJ-001", "Portable Projector", isAvailable: true)
        };

        services.AddSingleton<IStudentRepository>(new InMemoryStudentRepository(students));
        services.AddSingleton<IEquipmentRepository>(new InMemoryEquipmentRepository(equipment));
        services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();

        services.AddTransient<EquipmentViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
<Application.DataTemplates>
    <local:ViewLocator xmlns:local="using:EquipmentBorrowing.Desktop" />
</Application.DataTemplates>