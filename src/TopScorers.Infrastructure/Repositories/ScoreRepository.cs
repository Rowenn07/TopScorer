using Microsoft.EntityFrameworkCore;
using TopScorers.Core.Models;
using TopScorers.Core.Services;
using TopScorers.Infrastructure.Data;

namespace TopScorers.Infrastructure.Repositories;

public class ScoreRepository(ScoresContext context) : IScoreRepository
{
    public async Task AddScoresAsync(IEnumerable<PersonScore> scores, CancellationToken cancellationToken = default)
    {
        await context.Scores.AddRangeAsync(scores, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PersonScore>> GetTopScorersAsync(CancellationToken cancellationToken = default)
    {
        var topScore = await context.Scores.MaxAsync(s => (int?)s.Score, cancellationToken);
        if (topScore is null)
        {
            return Array.Empty<PersonScore>();
        }

        return await context.Scores
            .Where(s => s.Score == topScore.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonScore?> GetScoreAsync(string firstName, string secondName, CancellationToken cancellationToken = default)
    {
        var normalizedFirstName = firstName.ToLower();
        var normalizedSecondName = secondName.ToLower();
        
        return await context.Scores
            .AsNoTracking()
            .Where(s => s.FirstName.ToLower() == normalizedFirstName &&
                       s.SecondName.ToLower() == normalizedSecondName)
            .OrderByDescending(s => s.Score)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

