using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace LIOSCare.DoctorDashboard.Infrastructure.Persistence;

public sealed class DoctorPortalDbContextFactory : IDesignTimeDbContextFactory<DoctorPortalDbContext>
{
    public DoctorPortalDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DoctorPortalDb")
            ?? "Host=localhost;Port=5432;Database=social_platform_db;Username=postgres;Password=postgres;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<DoctorPortalDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history_doctor_dashboard", "provider"))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new DoctorPortalDbContext(options);
    }
}
