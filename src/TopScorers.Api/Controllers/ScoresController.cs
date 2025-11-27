using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopScorers.Api.Filters;
using TopScorers.Api.Models.Requests;
using TopScorers.Api.Models.Responses;
using TopScorers.Core.Models;
using TopScorers.Core.Services;

namespace TopScorers.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ScoresController : ControllerBase
{
    private readonly IScoreService _scoreService;
    private readonly ILogger<ScoresController> _logger;

    public ScoresController(IScoreService scoreService, ILogger<ScoresController> logger)
    {
        _scoreService = scoreService;
        _logger = logger;
    }
    [HttpPost]
    [ApiKeyAuthorize]
    [ProducesResponseType(typeof(ScoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateScore([FromBody] ScoreRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new PersonScore
            {
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Score = request.Score
            };

            await _scoreService.IngestScoresAsync(new[] { entry }, cancellationToken);

            var response = new ScoreResponse(entry.FirstName, entry.SecondName, entry.Score);
            return CreatedAtAction(nameof(GetScore), new { firstName = entry.FirstName, secondName = entry.SecondName }, response);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating score");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while saving the score.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating score");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    [HttpGet]
    [ApiKeyAuthorize]
    [ProducesResponseType(typeof(ScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ScoreResponse>> GetScore(
        [FromQuery, Required, MinLength(1)] string firstName, 
        [FromQuery, Required, MinLength(1)] string secondName, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Both firstName and secondName are required and cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var result = await _scoreService.GetScoreAsync(firstName, secondName, cancellationToken);
            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"No score found for {firstName} {secondName}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return new ScoreResponse(result.FirstName, result.SecondName, result.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving score for {FirstName} {SecondName}", firstName, secondName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving the score.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    [HttpGet("top")]
    [ApiKeyAuthorize]
    [ProducesResponseType(typeof(TopScoresResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopScoresResponse>> GetTopScores(CancellationToken cancellationToken)
    {
        try
        {
            var topScores = await _scoreService.GetTopScorersAsync(cancellationToken);
            if (topScores.Count == 0)
            {
                return Ok(new TopScoresResponse(0, Array.Empty<PersonNameResponse>()));
            }

            var score = topScores.First().Score;
            var people = topScores
                .Select(s => new PersonNameResponse(s.FirstName, s.SecondName))
                .ToList();

            return new TopScoresResponse(score, people);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top scores");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving top scores.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}

