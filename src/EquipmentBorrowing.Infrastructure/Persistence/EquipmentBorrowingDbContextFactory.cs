using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();

        // Default local development connection string (configurable via environment variable)
        string connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=frias";

        optionsBuilder.UseNpgsql(connectionString);

        return new EquipmentBorrowingDbContext(optionsBuilder.Options);
    }
}