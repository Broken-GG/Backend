using DotNetEnv;
using api.service;

Console.WriteLine("🚀 Starting Riot API Backend Server...");
Console.WriteLine("======================================");

// Load environment variables from .env file
Console.WriteLine("📁 Loading environment variables...");
Env.Load("src/.env");

// Build the application
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Register your services
builder.Services.AddScoped<RIOTAPI>();
builder.Services.AddScoped<IChampionDataService, ChampionDataService>();
builder.Services.AddScoped<IGameDataService, GameDataService>(provider =>
    new GameDataService(provider.GetRequiredService<IChampionDataService>()));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🌐 Backend server is running!");
Console.WriteLine("� Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("🔗 API Base URL: http://localhost:5000/api");
Console.WriteLine("� Frontend can now call your backend!");
Console.WriteLine();
Console.WriteLine("📋 Available Endpoints:");
Console.WriteLine("   📊 GET /api/SummonerInfo/{name}/{tag} - Get summoner information");
Console.WriteLine("   🎯 GET /api/MatchInfo/{puuid} - Get last 10 matches with win/loss, players, and champions");

app.Run();