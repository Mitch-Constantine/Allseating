using Microsoft.EntityFrameworkCore;
using Allseating.Application;
using Allseating.Infrastructure;
using Allseating.Infrastructure.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: default Error+ to JSON rolling file, optional console in Development
var logConfig = new LoggerConfiguration()
    .MinimumLevel.Error()
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day);

if (builder.Environment.IsDevelopment())
    logConfig.WriteTo.Console(Serilog.Events.LogEventLevel.Information);

Log.Logger = logConfig.CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext and database selection from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=AllseatingDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ListGames>();
builder.Services.AddScoped<GetGame>();
builder.Services.AddScoped<CreateGame>();
builder.Services.AddScoped<UpdateGame>();
builder.Services.AddScoped<DeleteGame>();
builder.Services.AddScoped<SeedService>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyMethod().AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Create/update schema and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var seedPath = Path.Combine(AppContext.BaseDirectory, "seed", "games.seed.json");
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.SeedFromFileIfEmptyAsync(seedPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();

public partial class Program { }
