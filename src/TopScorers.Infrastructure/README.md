# TopScorers.Infrastructure

Data access layer using Entity Framework Core and SQL Server.

## Overview

This is the **infrastructure layer** containing:
- Entity Framework Core `DbContext`
- Database migrations
- Repository implementations
- Data access configuration

## Components

### Database Context

#### ScoresContext

EF Core DbContext for managing the Scores table:

```csharp
public class ScoresContext : DbContext
{
    public DbSet<PersonScore> Scores { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table: Scores
        // Columns: Id (PK), FirstName, SecondName, Score
        // Index: IX_Scores_Score
    }
}
```

### Repository

#### ScoreRepository

Implementation of `IScoreRepository` from Core:

```csharp
public class ScoreRepository : IScoreRepository
{
    Task<PersonScore?> GetScoreAsync(string firstName, string secondName, ...);
    Task<List<PersonScore>> GetTopScorersAsync(...);
    Task AddScoresAsync(IEnumerable<PersonScore> scores, ...);
}
```

**Performance optimizations**:
- Parameters normalized before SQL execution (enables index usage)
- `AsNoTracking()` for read-only queries
- `OrderByDescending` for efficient max score retrieval
- Batch inserts for multiple scores

## Database Schema

### Scores Table

| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | int | Primary Key, Identity |
| `FirstName` | nvarchar(100) | Not Null |
| `SecondName` | nvarchar(200) | Not Null |
| `Score` | int | Not Null |

### Indexes

- **IX_Scores_Score**: Non-clustered index on `Score` column
  - Enables efficient `ORDER BY Score DESC` queries
  - Critical for top scorer lookups

## Migrations

### Creating Migrations

```powershell
cd src/TopScorers.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../TopScorers.Cli
```

### Applying Migrations

#### Automatic (Development)
Database is auto-created and migrated on first run when running the CLI or API in development mode.

#### Manual (Production)
```powershell
cd src/TopScorers.Cli
dotnet ef database update --project ../TopScorers.Infrastructure
```

### Existing Migrations

- **20251125221728_InitialCreate**: Creates Scores table with index

## Configuration

### Connection String

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TopScorers;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Production Connection String

Use environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=TopScorers;User Id=app_user;Password=***"
```

Or Azure App Service configuration.

## Dependency Injection Setup

Register Infrastructure services:

```csharp
services.AddInfrastructureServices(configuration); // Extension method
```

This registers:
- `ScoresContext` with SQL Server provider
- `IScoreRepository` → `ScoreRepository`

## Performance Considerations

### Query Optimization

✅ **Parameter normalization**: Names converted to lowercase before SQL execution
```csharp
var normalizedFirstName = firstName.ToLower();
return await _context.Scores
    .Where(s => s.FirstName.ToLower() == normalizedFirstName)
    .FirstOrDefaultAsync();
```

✅ **Index usage**: Queries leverage `IX_Scores_Score` index for top scorer lookups

✅ **Read-only queries**: Use `AsNoTracking()` to avoid change tracking overhead

### Batch Operations

The `AddScoresAsync` method accepts multiple scores and inserts them in a single transaction.

## Dependencies

- **Microsoft.EntityFrameworkCore** (8.0.6)
- **Microsoft.EntityFrameworkCore.SqlServer** (8.0.6)
- **Microsoft.EntityFrameworkCore.Design** (8.0.6) - For migrations
- **TopScorers.Core** - Domain models and interfaces

## Project Structure

```
TopScorers.Infrastructure/
├── Data/
│   ├── ScoresContext.cs              # EF Core DbContext
│   └── ScoresContextFactory.cs       # Design-time factory
├── DependencyInjection/
│   └── InfrastructureServiceCollectionExtensions.cs
├── Migrations/
│   ├── 20251125221728_InitialCreate.cs
│   ├── 20251125221728_InitialCreate.Designer.cs
│   └── ScoresContextModelSnapshot.cs
├── Repositories/
│   └── ScoreRepository.cs            # Data access implementation
└── TopScorers.Infrastructure.csproj  # Project file
```

## Testing

For integration tests, consider:
- **In-memory database**: Use `UseInMemoryDatabase()` for fast tests
- **SQLite**: Use `UseSqlite()` for more realistic tests
- **Test containers**: Use Testcontainers for full SQL Server integration tests

## Related Projects

- [TopScorers.Core](../TopScorers.Core/README.md) - Defines `IScoreRepository` interface
- [TopScorers.Api](../TopScorers.Api/README.md) - Uses repositories via dependency injection
- [TopScorers.Cli](../TopScorers.Cli/README.md) - Uses repositories for data import
