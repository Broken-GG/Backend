# Backend - Broken.GG API
[![CI/CD Integration](https://github.com/Broken-GG/BrokenGG/actions/workflows/integration.yml/badge.svg)](https://github.com/Broken-GG/Backend/actions)

ASP.NET Core Web API for League of Legends match history tracking.

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

## 🔧 Technologies

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Swagger/OpenAPI** - API documentation
- **Newtonsoft.Json** - JSON serialization
- **DotNetEnv** - Environment configuration
- **xUnit** - Testing framework
- **Moq** - Mocking library

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Riot Games API Key

### Installation

1. **Navigate to Backend folder**
   ```bash
   cd Backend
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Set up environment variables**
   ```bash
   cp src/.env.example src/.env
   ```
   
   Edit `src/.env` and add your Riot API key:
   ```env
   RIOT_API_KEY=your_api_key_here
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will start on `http://localhost:5000`

### Development

```bash
# Watch mode (auto-reload)
dotnet watch run

# Build
dotnet build

# Run tests
dotnet test

# Clean
dotnet clean
```

## 📡 API Endpoints

### Summoner Information
- **GET** `/api/SummonerInfo/{name}/{tag}`
  - Fetch summoner profile data
  - Returns: SummonerInfo object

### Match History
- **GET** `/api/MatchInfo/{puuid}?start=0&count=10`
  - Get match history by PUUID
  - Returns: Array of MatchSummary

- **GET** `/api/MatchInfo/summoner/{name}/{tag}?start=0&count=10`
  - Get match history by summoner name
  - Returns: Array of MatchSummary

### Side Panel Data
- **GET** `/api/SidePanelInfo/ranked/{puuid}`
  - Get ranked information
  - Returns: Array of RankedInfo

- **GET** `/api/SidePanelInfo/mastery/{puuid}`
  - Get champion mastery data
  - Returns: Array of MasteryInfo

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

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

## 🔄 API Rate Limiting

Riot API has rate limits:
- Development key: 20 requests/second, 100 requests/2 minutes
- Production key: Higher limits (apply on Riot Developer Portal)

Consider implementing:
- Caching layer (Redis)
- Rate limiting middleware
- Request queuing

## 🚧 Future Improvements

- [ ] Add caching (Redis/Memory Cache)
- [ ] Implement health check endpoint
- [ ] Add request logging middleware
- [ ] Create response compression
- [ ] Add API versioning
- [ ] Implement global error handling
- [ ] Add authentication/authorization
- [ ] Database integration for historical data
- [ ] Add more comprehensive tests
- [ ] Implement retry policies for external APIs

## 📝 Notes

- Data Dragon version is automatically fetched and cached
- PUUID is kept internal for security
- All date times are in UTC
- Match data is returned with both individual and team stats

## 🤝 Contributing

1. Follow C# coding conventions
2. Add tests for new features
3. Update Swagger documentation
4. Keep controllers thin
5. Use async/await for I/O operations
6. Handle exceptions appropriately
7. Add XML documentation comments

## 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Riot Games API Documentation](https://developer.riotgames.com/)
- [Data Dragon Documentation](https://riot-api-libraries.readthedocs.io/en/latest/ddragon.html)
