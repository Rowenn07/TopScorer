using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using TopScorers.Core.DependencyInjection;
using TopScorers.Core.Parsing;
using TopScorers.Core.Services;
using TopScorers.Infrastructure.Data;
using TopScorers.Infrastructure.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddCoreServices()
    .AddInfrastructure(builder.Configuration);

var host = builder.Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("TopScorers.Cli");

var cancellationToken = CancellationToken.None;
string? csvPath = null;

try
{
    await ApplyMigrationsIfNeededAsync(services, builder.Configuration, cancellationToken);

    csvPath = ResolveCsvPath(args, logger);
    if (csvPath is null)
    {
        return;
    }

    if (!File.Exists(csvPath))
    {
        logger.LogError("CSV file '{Path}' does not exist.", csvPath);
        return;
    }

    var csvContent = await File.ReadAllTextAsync(csvPath, cancellationToken);
    var parser = services.GetRequiredService<ICsvParser>();
    var people = parser.Parse(csvContent).ToList();
    if (people.Count == 0)
    {
        logger.LogWarning("No records were parsed from the file.");
        return;
    }

    var scoreService = services.GetRequiredService<IScoreService>();
    await scoreService.IngestScoresAsync(people, cancellationToken);
    var topScorers = await scoreService.GetTopScorersAsync(cancellationToken);
    if (topScorers.Count == 0)
    {
        logger.LogInformation("No scores are stored in the database yet.");
        return;
    }

    foreach (var person in topScorers)
    {
        Console.WriteLine($"{person.FirstName} {person.SecondName}".Trim());
    }

    Console.WriteLine($"Score: {topScorers.First().Score}");
}
catch (IOException ex)
{
    logger.LogError(ex, "Failed to read CSV file: {Path}", csvPath ?? "unknown");
    Console.WriteLine($"File Error: {ex.Message}");
}
catch (DbUpdateException ex)
{
    logger.LogError(ex, "Database error occurred while saving scores");
    Console.WriteLine($"Database Error: Failed to save scores. {ex.InnerException?.Message ?? ex.Message}");
}
catch (Exception ex)
{
    logger.LogError(ex, "An unexpected error occurred");
    Console.WriteLine($"Error: {ex.Message}");
}

static string? ResolveCsvPath(string[] args, ILogger logger)
{
    if (args.Length == 0)
    {
        logger.LogError("Please supply a path to a CSV file as the first argument.");
        return null;
    }

    var path = args[0];
    return Path.GetFullPath(path);
}

static async Task ApplyMigrationsIfNeededAsync(IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken)
{
    var autoMigrate = configuration.GetValue("TopScorers:AutoMigrate", true);
    if (!autoMigrate)
    {
        return;
    }

    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ScoresContext>();
    await dbContext.Database.MigrateAsync(cancellationToken);
}
