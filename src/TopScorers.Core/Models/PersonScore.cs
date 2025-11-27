namespace TopScorers.Core.Models;

/// <summary>
/// Domain representation of a person's score.
/// </summary>
public class PersonScore
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string SecondName { get; set; } = string.Empty;

    public int Score { get; set; }

    public string FullName => $"{FirstName} {SecondName}".Trim();
}

