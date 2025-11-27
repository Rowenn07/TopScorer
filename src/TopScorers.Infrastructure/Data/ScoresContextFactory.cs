using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TopScorers.Infrastructure.Data;

public class ScoresContextFactory : IDesignTimeDbContextFactory<ScoresContext>
{
    public ScoresContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ScoresDb")
            ?? "Server=localhost;Database=TopScorers;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<ScoresContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ScoresContext(optionsBuilder.Options);
    }
}

