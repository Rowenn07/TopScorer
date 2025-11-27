namespace TopScorers.Api.Models.Responses;

public record TopScoresResponse(int Score, IReadOnlyCollection<PersonNameResponse> People);

