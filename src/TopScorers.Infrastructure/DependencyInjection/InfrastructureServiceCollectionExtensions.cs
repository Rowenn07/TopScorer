using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopScorers.Core.Services;
using TopScorers.Infrastructure.Data;
using TopScorers.Infrastructure.Repositories;

namespace TopScorers.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ScoresContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ScoresDb")
                ?? throw new InvalidOperationException("Connection string 'ScoresDb' not found.");
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IScoreRepository, ScoreRepository>();
        return services;
    }
}

