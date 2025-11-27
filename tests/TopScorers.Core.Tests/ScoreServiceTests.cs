using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TopScorers.Core.Models;
using TopScorers.Core.Services;

namespace TopScorers.Core.Tests;

public class ScoreServiceTests
{
    [Fact]
    public async Task GetTopScorersAsync_ShouldReturnAlphabeticallyOrderedResults()
    {
        var repository = new FakeScoreRepository(new[]
        {
            new PersonScore { FirstName = "Sipho", SecondName = "Lolo", Score = 78 },
            new PersonScore { FirstName = "George", SecondName = "Of The Jungle", Score = 78 }
        });

        var service = new ScoreService(NullLogger<ScoreService>.Instance, repository);

        var result = await service.GetTopScorersAsync();

        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("George");
        result[1].FirstName.Should().Be("Sipho");
    }

    private sealed class FakeScoreRepository : IScoreRepository
    {
        private readonly IReadOnlyList<PersonScore> topScorers;

        public FakeScoreRepository(IReadOnlyList<PersonScore> topScorers)
        {
            this.topScorers = topScorers;
        }

        public Task AddScoresAsync(IEnumerable<PersonScore> scores, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<PersonScore?> GetScoreAsync(string firstName, string secondName, CancellationToken cancellationToken = default) =>
            Task.FromResult<PersonScore?>(topScorers.FirstOrDefault());

        public Task<IReadOnlyList<PersonScore>> GetTopScorersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(topScorers);
    }
}

