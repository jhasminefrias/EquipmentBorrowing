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

public partial class App : Avalonia.Application
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
        // so state (e.g. an equipment item becoming unavailable or borrowed)
        // survives navigation between views instead of resetting every time.
        var students = new[]
        {
            new Student(1, "2026-0001", "Jhasmine Frias", isAllowedToBorrow: true, maximumActiveBorrowings: 2),
            new Student(2, "2026-0002", "Hannah Rapal", isAllowedToBorrow: false, maximumActiveBorrowings: 2),
            new Student(3, "2026-0003", "Wendell Pasquil", isAllowedToBorrow: true, maximumActiveBorrowings: 1),
            new Student(4, "2026-0004", "Shanice Palasan", isAllowedToBorrow: false, maximumActiveBorrowings: 3),
            new Student(5, "2026-0005", "Airesh Abcd", isAllowedToBorrow: true, maximumActiveBorrowings: 1),
        };

        var equipment = new[]
        {
            new Equipment(1, "LAB-CAM-001", "Digital Camera", isAvailable: true),
            new Equipment(2, "LAB-MIC-001", "Wireless Microphone", isAvailable: false),
            new Equipment(3, "LAB-PROJ-001", "Portable Projector", isAvailable: true),
            new Equipment(4, "LAB-LAP-001", "Dell XPS Laptop", isAvailable: true),
            new Equipment(4, "LAB-LAP-001", "Dell XPS Laptop", isAvailable: true),
            new Equipment(4, "LAB-TRP-001", "Heavy-Duty Camera Tripod", isAvailable: true),
            new Equipment(5, "LAB-REC-001", "Digital Audio Field Recorder", isAvailable: true),
            new Equipment(6, "LAB-LAP-001", "Dell XPS 15 Laptop", isAvailable: true),
            new Equipment(7, "LAB-LAP-002", "MacBook Pro 14\"", isAvailable: true),
            new Equipment(8, "LAB-TAB-001", "Wacom Drawing Tablet", isAvailable: true),
        };

        var initialBorrowings = new[]
        {
            new Borrowing(
                1,
                1,
                2,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(5)))
        };

        // Repositories registered as singletons to preserve state across views
        services.AddSingleton<IStudentRepository>(new InMemoryStudentRepository(students));
        services.AddSingleton<IEquipmentRepository>(new InMemoryEquipmentRepository(equipment));
        services.AddSingleton<IBorrowingRepository>(new InMemoryBorrowingRepository(initialBorrowings));

        // Application services
        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        // ViewModels
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
