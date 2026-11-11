using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RaceDay.Data;

/// <summary>
/// Creates the EF Core context for migrations and migration-bundle generation
/// without starting the API host or requiring JWT/Azure runtime services.
/// </summary>
public sealed class RaceDayContextFactory : IDesignTimeDbContextFactory<RaceDayContext>
{
    public RaceDayContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("RaceDay", "appsettings.json"), optional: true)
            .AddUserSecrets(typeof(RaceDayContext).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("RaceDayConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:RaceDayConnection must be configured for EF Core migrations.");

        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new RaceDayContext(options);
    }
}
