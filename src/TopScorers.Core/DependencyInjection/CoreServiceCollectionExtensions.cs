using Microsoft.Extensions.DependencyInjection;
using TopScorers.Core.Parsing;
using TopScorers.Core.Services;

namespace TopScorers.Core.DependencyInjection;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ICsvParser, CsvParser>();
        services.AddScoped<IScoreService, ScoreService>();
        return services;
    }
}

