using DotNetEnv;
using api.service;

Console.WriteLine("🚀 Starting Riot API Backend Server...");
Console.WriteLine("======================================");

// Load environment variables from .env file
Console.WriteLine("📁 Loading environment variables...");
Env.Load("src/.env");

// Build the application
var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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
Console.WriteLine("📖 Swagger UI: https://localhost:5001/swagger");
Console.WriteLine("🔗 API Base URL: https://localhost:5001/api");
Console.WriteLine("🎮 Frontend can now call your backend!");

app.Run();