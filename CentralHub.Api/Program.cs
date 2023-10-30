using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Use state directory given by systemd
var stateDirectory = Environment.GetEnvironmentVariable("STATE_DIRECTORY") ?? Environment.CurrentDirectory;
Console.WriteLine($"State directory: {stateDirectory}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkSqlite();

#if DEBUG
var id = $"{Guid.NewGuid().ToString()}.db";

var sqliteStringBuilder = new SqliteConnectionStringBuilder()
{
    DataSource = id,
    Mode = SqliteOpenMode.Memory,
    Cache = SqliteCacheMode.Shared
};

var connectionString = sqliteStringBuilder.ToString();
#else
var connectionString = $"Data Source={Path.Combine(stateDirectory, "data.db")}";
#endif

builder.Services.AddDbContext<TrackersContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Singleton);
builder.Services.AddSingleton<ITrackersRepository, TrackersRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Insert dummy trackers
    var trackersRepository = app.Services.GetRequiredService<ITrackersRepository>();
    trackersRepository.AddTracker(new Tracker("Test Tracker 1", "AA:BB:CC:DD:EE:FF", TrackerType.WiFi));
    trackersRepository.AddTracker(new Tracker("Test Tracker 2", "FF:EE:DD:CC:BB:AA", TrackerType.WiFi));
    trackersRepository.AddTracker(new Tracker("Test Tracker 3", "00:11:22:33:44:55", TrackerType.Bluetooth));
}


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
