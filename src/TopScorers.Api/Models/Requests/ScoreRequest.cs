using System.ComponentModel.DataAnnotations;

namespace TopScorers.Api.Models.Requests;

public class ScoreRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SecondName { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Score { get; set; }
}

