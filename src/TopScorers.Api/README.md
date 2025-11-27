# TopScorers.Api

ASP.NET Core Web API for managing and querying score data.

## Overview

The API provides RESTful endpoints to:
- Import scores from CSV content
- Query individual scores by person name
- Retrieve top scorers

## Endpoints

| Endpoint | Method | Auth Required | Description |
|----------|--------|--------------|-------------|
| `/api/v1/scores` | POST | ✅ Yes | Import scores from CSV content |
| `/api/v1/scores` | GET | ✅ Yes | Get score for specific person |
| `/api/v1/scores/top` | GET | ✅ Yes | Get top scorers |

## Running the API

### Development Mode (HTTP)

```powershell
cd src/TopScorers.Api
dotnet run
```

API will be available at: `http://localhost:5080`
Swagger UI: `http://localhost:5080/swagger`

### With HTTPS

```powershell
dotnet run --launch-profile https
```

API will be available at: `https://localhost:7281`
Swagger UI: `https://localhost:7281/swagger`

## Configuration

### API Key Setup

API key is configured in `appsettings.json`:

```json
{
  "Security": {
    "ApiKey": "local-dev-key"
  }
}
```

For production, use environment variables:

```bash
export Security__ApiKey="your-production-key"
```

### Database Connection

Connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TopScorers;..."
  }
}
```

## API Usage Examples

### Import Scores

```powershell
$headers = @{ "x-api-key" = "local-dev-key" }
$body = @{
    firstName = "John"
    secondName = "Doe"
    score = 95
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5080/api/v1/scores" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -ContentType "application/json"
```

### Get Specific Person's Score

```powershell
$headers = @{ "x-api-key" = "local-dev-key" }
Invoke-RestMethod -Uri "http://localhost:5080/api/v1/scores?firstName=John&secondName=Doe" `
    -Method Get `
    -Headers $headers
```

### Get Top Scorers

```powershell
$headers = @{ "x-api-key" = "local-dev-key" }
Invoke-RestMethod -Uri "http://localhost:5080/api/v1/scores/top" `
    -Method Get `
    -Headers $headers
```

## Security

- **Authentication**: Custom API key via `x-api-key` header
- **Authorization**: All endpoints require valid API key
- **Transport Security**: HTTPS recommended for production
- **Audit Logging**: Unauthorized attempts logged with IP address

## Error Handling

API returns RFC 7807 compliant `ProblemDetails` objects:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid API key
- `404 Not Found` - Person not found
- `500 Internal Server Error` - Server errors

## Dependencies

- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core 8.0** - Database access
- **Swashbuckle** - OpenAPI/Swagger documentation
- **TopScorers.Core** - Business logic
- **TopScorers.Infrastructure** - Data access

## Project Structure

```
TopScorers.Api/
├── Controllers/
│   └── ScoresController.cs       # API endpoints
├── Filters/
│   ├── ApiKeyAuthorizeAttribute.cs   # Custom auth filter
│   └── ApiKeyOperationFilter.cs      # Swagger documentation
├── Models/
│   ├── Requests/                 # Request DTOs
│   └── Responses/                # Response DTOs
├── Properties/
│   └── launchSettings.json       # Launch profiles
├── appsettings.json              # Configuration
└── Program.cs                    # App startup
```

## Related Projects

- [TopScorers.Core](../TopScorers.Core/README.md) - Business logic and models
- [TopScorers.Infrastructure](../TopScorers.Infrastructure/README.md) - Database and repositories
- [TopScorers.Cli](../TopScorers.Cli/README.md) - Command-line CSV importer
