using Microsoft.Extensions.Logging;
using TopScorers.Core.Models;

namespace TopScorers.Core.Services;

public class ScoreService(ILogger<ScoreService> logger, IScoreRepository repository) : IScoreService
{
    public async Task IngestScoresAsync(IEnumerable<PersonScore> scores, CancellationToken cancellationToken = default)
    {
        var scoresArray = scores.ToArray();
        var sanitized = scoresArray
            .Where(s => !string.IsNullOrWhiteSpace(s.FirstName) && !string.IsNullOrWhiteSpace(s.SecondName))
            .Select(s => new PersonScore
            {
                FirstName = s.FirstName.Trim(),
                SecondName = s.SecondName.Trim(),
                Score = s.Score
            })
            .ToList();

        var skippedCount = scoresArray.Length - sanitized.Count;
        
        if (sanitized.Count == 0)
        {
            logger.LogInformation("No valid scores to ingest. Skipped {SkippedCount} invalid records", skippedCount);
            return;
        }

        await repository.AddScoresAsync(sanitized, cancellationToken);
        logger.LogInformation("Successfully ingested {IngestedCount} scores, skipped {SkippedCount} invalid records", 
            sanitized.Count, skippedCount);
    }

    public async Task<IReadOnlyList<PersonScore>> GetTopScorersAsync(CancellationToken cancellationToken = default)
    {
        var topScorers = await repository.GetTopScorersAsync(cancellationToken);
        return topScorers
            .OrderBy(s => s.FirstName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.SecondName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public Task<PersonScore?> GetScoreAsync(string firstName, string secondName, CancellationToken cancellationToken = default)
    {
        var normalizedFirst = firstName.Trim();
        var normalizedSecond = secondName.Trim();
        return repository.GetScoreAsync(normalizedFirst, normalizedSecond, cancellationToken);
    }
}

