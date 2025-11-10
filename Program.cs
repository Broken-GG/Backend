using DotNetEnv;
using Backend.Extensions;
using Backend.Middleware;

Console.WriteLine("🚀 Starting Riot API Backend Server...");
Console.WriteLine("======================================");

// Load environment variables from .env file
Console.WriteLine("📁 Loading environment variables...");
Env.Load(".env");

// Build the application
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add memory cache for Data Dragon data
builder.Services.AddMemoryCache();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<Backend.Services.RiotApiHealthCheck>("riot_api");

// Add CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Register application services using extension methods
builder.Services.AddApplicationServices(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

Console.WriteLine("🌐 Backend server is running!");
Console.WriteLine("📖 Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("🔗 API Base URL: http://localhost:5000/api");
Console.WriteLine("🌍 Frontend can now call your backend!");
Console.WriteLine();
Console.WriteLine("📋 Available Endpoints:");
Console.WriteLine("   ❤️  GET /api/Health - Health check");
Console.WriteLine("   📊 GET /api/Summoner/{name}/{tag} - Get summoner information");
Console.WriteLine("   🎯 GET /api/Match/{puuid} - Get match history by PUUID");
Console.WriteLine("   🎯 GET /api/Match/summoner/{name}/{tag} - Get match history by summoner");
Console.WriteLine("   📈 GET /api/Ranked/{puuid} - Get ranked information");
Console.WriteLine("   🏆 GET /api/Mastery/{puuid} - Get mastery information");

app.Run();