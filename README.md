# 🔌 Backend - Broken.GG API

[![CI/CD Pipeline](https://github.com/Broken-GG/Backend/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Broken-GG/Backend/actions)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> ASP.NET Core 9.0 Web API for League of Legends match history tracking, powered by Riot Games API.

## 🏗 Architecture

### Project Structure
```
Backend/
├── src/
│   ├── api/
│   │   ├── controller/          # API Controllers (HTTP endpoints)
│   │   │   ├── SummonerInfoController.cs
│   │   │   ├── MatchInfoController.cs
│   │   │   └── SidePanelInfoController.cs
│   │   ├── models/              # Data models (DTOs)
│   │   │   ├── Summoner.cs
│   │   │   ├── MatchSummary.cs
│   │   │   ├── MatchHistory.cs
│   │   │   ├── RankedInfo.cs
│   │   │   └── MasteryInfo.cs
│   │   ├── service/             # Business logic & External APIs
│   │   │   ├── RIOTAPI.cs
│   │   │   ├── ChampionDataService.cs
│   │   │   └── GameDataService.cs
│   │   └── .env                 # Environment variables (gitignored)
│   └── test/
│       └── IntegrationTests.cs  # Unit & Integration tests
├── Program.cs                   # Application entry point
├── Backend.csproj               # Project configuration
├── Dockerfile                   # Container configuration
└── README.md                    # This file
```

## 🛠 Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 9.0 | Core framework |
| **ASP.NET Core** | 9.0 | Web API |
| **C#** | 12.0 | Language |
| **Swagger/OpenAPI** | 6.8+ | API docs |
| **xUnit** | 2.9+ | Testing |
| **Moq** | 4.20+ | Mocking |
| **DotNetEnv** | 3.0+ | Config management |

### External APIs
- **Riot Games API** - Player and match data
- **Data Dragon** - Static game assets

## 🚀 Quick Start

### Prerequisites

```bash
# Check .NET version (requires 9.0+)
dotnet --version

# Get Riot API key
# Visit: https://developer.riotgames.com/
```

### Installation

1. **Clone and navigate**
   ```bash
   git clone https://github.com/Broken-GG/Backend.git
   cd Backend
   ```

2. **Configure environment**
   ```bash
   # Create .env file
   cp .env.example .env
   
   # Add your Riot API key
   echo "RIOT_API_KEY=your_key_here" >> .env
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Verify it's running**
   ```bash
   curl http://localhost:5000/api/health
   ```

### Development Commands

```bash
# Run with hot reload
dotnet watch run

# Build for release
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Format code
dotnet format

# Clean build artifacts
dotnet clean
```

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| **API** | http://localhost:5000 | Base API endpoint |
| **Swagger UI** | http://localhost:5000/swagger | Interactive API docs |
| **Health Check** | http://localhost:5000/api/health | Service health |

## 📡 API Endpoints

### Health Check
```http
GET /api/health
```
Returns service health status and Riot API connectivity.

### Summoner Endpoints

<details>
<summary><b>GET</b> /api/summoner/{name}/{tag}</summary>

**Description:** Fetch summoner profile information

**Parameters:**
- `name` - Summoner name (e.g., "Faker")
- `tag` - Tagline (e.g., "T1")

**Example:**
```bash
curl http://localhost:5000/api/summoner/Faker/T1
```

**Response:**
```json
{
  "summonerName": "Faker",
  "tagline": "T1",
  "puuid": "abc123...",
  "level": 623,
  "profileIconUrl": "https://..."
}
```
</details>

### Match Endpoints

<details>
<summary><b>GET</b> /api/match/{puuid}</summary>

**Description:** Get match history by PUUID

**Parameters:**
- `puuid` - Player UUID
- `start` - Offset (default: 0)
- `count` - Number of matches (default: 10, max: 20)

**Example:**
```bash
curl "http://localhost:5000/api/match/abc123?start=0&count=5"
```

</details>

<details>
<summary><b>GET</b> /api/match/summoner/{name}/{tag}</summary>

**Description:** Get match history by summoner name

**Example:**
```bash
curl "http://localhost:5000/api/match/summoner/Faker/T1?count=10"
```

</details>

### Ranked & Mastery Endpoints

<details>
<summary><b>GET</b> /api/ranked/{puuid}</summary>

Returns ranked information for all queues (Solo/Duo, Flex, etc.)

</details>

<details>
<summary><b>GET</b> /api/mastery/{puuid}</summary>

Returns top champion masteries with levels and points.

</details>

**Full documentation:** Visit `/swagger` when API is running

## 🧪 Testing

### Run Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

```
Test/
├── Unit/
│   ├── Controllers/      # Controller tests
│   ├── Services/         # Service tests
│   └── Helpers/          # Helper tests
└── Integration/
    ├── EndToEndApiTests.cs
    ├── HealthCheckTests.cs
    └── RateLimitingTests.cs
```

### CI/CD Tests

Tests run automatically on:
- Every push to `main` or `develop`
- All pull requests
- Manual workflow dispatch

## 🐳 Docker

### Build
```bash
docker build -t brokengg-backend .
```

### Run
```bash
docker run -p 5000:5000 --env-file src/.env brokengg-backend
```

## 🔐 Configuration

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| RIOT_API_KEY | Riot Games API Key | - | ✅ |
| RIOT_API_URL | Account API endpoint | europe.api.riotgames.com/... | ❌ |
| RIOT_SUMMONER_URL | Summoner API endpoint | euw1.api.riotgames.com/... | ❌ |
| ASPNETCORE_URLS | Server URL | http://+:5000 | ❌ |
| ASPNETCORE_ENVIRONMENT | Environment mode | Development | ❌ |

### CORS Configuration

Currently set to allow all origins for development. Update in `Program.cs` for production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder
            .WithOrigins("https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

## 📊 Code Structure

### Controllers
Handle HTTP requests and responses. Keep them thin - delegate to services.

### Services
- **RIOTAPI**: Handles all Riot API communication
- **ChampionDataService**: Manages champion data from Data Dragon
- **GameDataService**: Handles items and summoner spells

### Models
Data Transfer Objects (DTOs) representing API contracts.

## ⚡ Performance & Rate Limiting

### Riot API Rate Limits

| Key Type | Limits |
|----------|--------|
| **Development** | 20 req/sec, 100 req/2min |
| **Production** | Higher limits ([Apply here](https://developer.riotgames.com/)) |

### Implemented Features
- ✅ Error handling middleware
- ✅ Request logging
- ✅ Health checks
- ⏳ Caching (planned)
- ⏳ Rate limiting middleware (planned)

## 🤝 Contributing

### Code Standards

```csharp
// ✅ Good: Async all the way
public async Task<SummonerResponse> GetSummonerAsync(string name, string tag)
{
    return await _riotApiService.GetSummonerAsync(name, tag);
}

// ❌ Bad: Sync over async
public SummonerResponse GetSummoner(string name, string tag)
{
    return _riotApiService.GetSummonerAsync(name, tag).Result;
}
```

### Pull Request Checklist

- [ ] Code follows C# conventions
- [ ] Tests added/updated
- [ ] Swagger docs updated
- [ ] No compiler warnings
- [ ] All tests pass locally
- [ ] PR description explains changes

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
feat(summoner): add regional endpoint support
fix(match): resolve null reference in team stats
docs(readme): update API endpoint examples
test(ranked): add integration tests for ranked endpoints
```

## 🐛 Troubleshooting

<details>
<summary><b>401 Unauthorized from Riot API</b></summary>

- Check `RIOT_API_KEY` in `.env`
- Development keys expire after 24 hours
- Regenerate at [developer.riotgames.com](https://developer.riotgames.com/)

</details>

<details>
<summary><b>Tests failing with "Connection refused"</b></summary>

Integration tests need Riot API key:
```bash
# Add to .env
RIOT_API_KEY=your_key_here

# Or skip integration tests
dotnet test --filter "FullyQualifiedName~Unit"
```

</details>

<details>
<summary><b>Port 5000 already in use</b></summary>

```bash
# Change port in launchSettings.json or use environment variable
export ASPNETCORE_URLS="http://localhost:5001"
dotnet run
```

</details>

## 📚 Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Riot Games API](https://developer.riotgames.com/)
- [Data Dragon](https://riot-api-libraries.readthedocs.io/en/latest/ddragon.html)
- [xUnit Testing](https://xunit.net/)

## 🎯 Roadmap

- [x] Basic API endpoints
- [x] Swagger documentation
- [x] Health checks
- [x] Error handling
- [x] Integration tests
- [x] Docker support
- [x] CI/CD pipeline
- [ ] Redis caching
- [ ] Rate limiting middleware
- [ ] Database integration
- [ ] Authentication/Authorization
- [ ] API versioning
- [ ] Response compression
- [ ] Request validation
- [ ] Retry policies

## 📝 License

MIT License - See [LICENSE](LICENSE) file

## 🙏 Acknowledgments

- Riot Games for providing the API
- Community contributors
- ASP.NET Core team

---

<p align="center">Part of <a href="https://github.com/Broken-GG/BrokenGG">Broken.GG</a> project</p>
