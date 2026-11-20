using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Initialize and seed database if empty
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();
            await DbInitializer.InitializeAsync(dbContext);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 1. PostgreSQL Connection
        string connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=frias";

        services.AddDbContext<EquipmentBorrowingDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, Microsoft.Extensions.Logging.LogLevel.Information));

        // 2. Repositories & Unit of Work
        services.AddScoped<IStudentRepository, EfStudentRepository>();
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. Application Services
        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();
        services.AddTransient<StudentManagementService>();
        services.AddTransient<EquipmentManagementService>();
        services.AddTransient<CheckOutMultipleEquipmentService>();

        // 4. ViewModels
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }
}
