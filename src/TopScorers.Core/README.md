# TopScorers.Core

Core business logic and domain models for the TopScorers application.

## Overview

This is the **domain layer** containing:
- Domain models (entities)
- Business logic and services
- Custom CSV parser (no external libraries)
- Service interfaces

**Key principle**: No dependencies on infrastructure, UI, or external frameworks. Pure .NET/C# code.

## Components

### Models

#### PersonScore
```csharp
public class PersonScore
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Score { get; set; }
}
```

Domain entity representing a person and their score.

### Services

#### IScoreService / ScoreService

Business logic for score management:

```csharp
public interface IScoreService
{
    Task IngestScoresAsync(IEnumerable<PersonScore> scores, CancellationToken cancellationToken);
    Task<PersonScore?> GetScoreAsync(string firstName, string secondName, CancellationToken cancellationToken);
    Task<List<PersonScore>> GetTopScorersAsync(CancellationToken cancellationToken);
}
```

**Key features**:
- Validates and ingests score data
- Retrieves individual scores
- Finds top scorers with alphabetical sorting
- Comprehensive logging of operations

### CSV Parser

#### ICsvParser / CsvParser

Custom CSV parser built from scratch (no external libraries):

```csharp
public interface ICsvParser
{
    Task<List<PersonScore>> ParseScoresAsync(string csvContent);
}
```

**Features**:
- Character-by-character state machine
- Handles quoted fields: `"Smith, John"`
- Handles escaped quotes: `"He said ""Hello"""`
- Flexible header mapping (case-insensitive, handles spaces/underscores)
- Skips invalid rows and logs summary
- No dependencies on `CsvHelper`, `TextFieldParser`, or similar

**Why custom parser?**
- Assessment requirement: "without using any CSV parsing libraries"
- Demonstrates understanding of state machines and text parsing
- Full control over parsing behavior and error handling

## Design Principles

### Clean Architecture

- **No infrastructure dependencies**: Core doesn't reference databases, HTTP, or frameworks
- **Dependency inversion**: Interfaces defined here, implemented elsewhere
- **Testability**: Pure logic, easy to unit test

### Business Rules

1. **Name validation**: First and second names must not be empty (after trimming)
2. **Score validation**: Must be a valid integer
3. **Top scorers**: Returns all people tied for highest score, alphabetically sorted
4. **Case-insensitive lookups**: Names are queried case-insensitively

## Dependencies

- **Microsoft.Extensions.Logging.Abstractions** - Logging interfaces only
- No database, HTTP, or infrastructure dependencies

This keeps the Core project framework-agnostic and focused on business logic.

## Dependency Injection Setup

Register Core services in your application:

```csharp
services.AddCoreServices(); // Extension method from CoreServiceCollectionExtensions
```

This registers:
- `ICsvParser` → `CsvParser`
- `IScoreService` → `ScoreService`

## Testing

Unit tests for Core logic are in `tests/TopScorers.Core.Tests`:

- `CsvParserTests` - CSV parsing scenarios
- `ScoreServiceTests` - Business logic validation

Tests use:
- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions
- **Moq** - Mocking dependencies

## Project Structure

```
TopScorers.Core/
├── DependencyInjection/
│   └── CoreServiceCollectionExtensions.cs  # Service registration
├── Models/
│   └── PersonScore.cs                      # Domain entity
├── Parsing/
│   ├── ICsvParser.cs                       # Parser interface
│   └── CsvParser.cs                        # Custom CSV parser
├── Services/
│   ├── IScoreRepository.cs                 # Repository interface
│   ├── IScoreService.cs                    # Service interface
│   └── ScoreService.cs                     # Business logic
└── TopScorers.Core.csproj                  # Project file
```

## Usage Examples

### CSV Parsing

```csharp
var parser = serviceProvider.GetRequiredService<ICsvParser>();
string csvContent = await File.ReadAllTextAsync("scores.csv");
List<PersonScore> scores = await parser.ParseScoresAsync(csvContent);
```

### Score Operations

```csharp
var scoreService = serviceProvider.GetRequiredService<IScoreService>();

// Import scores
await scoreService.IngestScoresAsync(scores, cancellationToken);

// Get specific person's score
var score = await scoreService.GetScoreAsync("John", "Doe", cancellationToken);

// Get top scorers
var topScorers = await scoreService.GetTopScorersAsync(cancellationToken);
```

## Related Projects

- [TopScorers.Infrastructure](../TopScorers.Infrastructure/README.md) - Implements `IScoreRepository`
- [TopScorers.Api](../TopScorers.Api/README.md) - Exposes services via REST API
- [TopScorers.Cli](../TopScorers.Cli/README.md) - Console application using Core services
