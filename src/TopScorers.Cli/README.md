# TopScorers.Cli

Command-line tool to import CSV files and display top scorers.

## Overview

Console application that:
- Reads a CSV file of people and their scores
- Imports the data into SQL Server database
- Outputs the top scorer(s) to STDOUT

## Usage

### Basic Usage

```powershell
cd src/TopScorers.Cli
dotnet run -- path/to/scores.csv
```

### Using TestData.csv

```powershell
# From solution root
cd src/TopScorers.Cli
dotnet run -- ../../TestData.csv
```

### Expected Output

```
George Of The Jungle
Sipho Lolo
Score: 78
```

The output format:
- First line(s): Names of people with the top score (alphabetically sorted)
- Last line: `Score: <value>`

## CSV Format Requirements

### Expected Columns

The CSV must contain at least these columns (order doesn't matter):
- `FirstName` or `First Name` or similar
- `SecondName` or `Second Name` or `Surname` or similar
- `Score` (numeric)

### Supported Features

- **Headers**: First row is treated as headers
- **Flexible naming**: Column names are case-insensitive and handle spaces/underscores
- **Quoted fields**: Handles fields with commas in quotes: `"Last, First"`
- **Escaped quotes**: Handles quotes within quotes: `"He said ""Hello"""`
- **Missing data**: Skips rows with empty names or invalid scores

### Example CSV

```csv
FirstName,SecondName,Score
John,Doe,85
Jane,Smith,90
Bob,"O'Brien, Jr.",90
Alice,Johnson,78
```

## Configuration

### Database Connection

Connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TopScorers;..."
  }
}
```

### Auto-Migration

In development, the database is created and migrated automatically on first run. No manual setup required.

## Error Handling

The CLI handles common errors gracefully:

### File Not Found
```
Error: The file 'path/to/file.csv' was not found.
```

### Invalid CSV Format
Rows with invalid data are skipped and logged:
```
Skipped 2 rows with invalid data.
Imported 48 scores successfully.
```

### Database Errors
```
Error: Failed to save scores to database.
[Details logged to console]
```

## How It Works

1. **Parse CSV**: Custom parser reads the file character-by-character (no CSV library)
2. **Import Scores**: Inserts or updates records in the `Scores` table
3. **Query Top Scorers**: Finds highest score and all people with that score
4. **Output Results**: Prints names (alphabetically) and score to STDOUT

## Performance Notes

- Uses batch inserts for efficiency
- Skips duplicate entries (same first name + second name)
- Database operations are transactional

## Dependencies

- **.NET 8.0** - Runtime
- **Entity Framework Core 8.0** - Database access
- **TopScorers.Core** - Business logic and CSV parser
- **TopScorers.Infrastructure** - Database context and repositories

## Project Structure

```
TopScorers.Cli/
├── Program.cs              # Main entry point
├── appsettings.json        # Configuration
└── TopScorers.Cli.csproj   # Project file
```

## Related Projects

- [TopScorers.Core](../TopScorers.Core/README.md) - Business logic and CSV parser
- [TopScorers.Infrastructure](../TopScorers.Infrastructure/README.md) - Database access
- [TopScorers.Api](../TopScorers.Api/README.md) - REST API for score management
