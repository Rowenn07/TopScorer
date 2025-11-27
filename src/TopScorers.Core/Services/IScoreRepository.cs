using TopScorers.Core.Models;

namespace TopScorers.Core.Services;

public interface IScoreRepository
{
    Task AddScoresAsync(IEnumerable<PersonScore> scores, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PersonScore>> GetTopScorersAsync(CancellationToken cancellationToken = default);

    Task<PersonScore?> GetScoreAsync(string firstName, string secondName, CancellationToken cancellationToken = default);
}

