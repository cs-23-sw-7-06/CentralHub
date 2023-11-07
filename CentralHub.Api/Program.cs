using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Use state directory given by systemd
var stateDirectory = Environment.GetEnvironmentVariable("STATE_DIRECTORY") ?? Environment.CurrentDirectory;
Console.WriteLine($"State directory: {stateDirectory}");

var builder = WebApplication.CreateBuilder(args);

// Better SystemD integration
builder.Host.UseSystemd();

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

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Singleton);
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Insert dummy trackers
    var roomRepository = app.Services.GetRequiredService<IRoomRepository>();
    var room = new Room("Test Room 1", "Test Room");
    roomRepository.AddRoomAsync(room, default).GetAwaiter().GetResult();
    roomRepository.AddTrackerAsync(new Tracker("Test Tracker 1", "Test Tracker", "AA:BB:CC:DD:EE:FF", room), default).GetAwaiter().GetResult();
    roomRepository.AddTrackerAsync(new Tracker("Test Tracker 2", "Test Tracker", "FF:EE:DD:CC:BB:AA", room), default).GetAwaiter().GetResult();
    roomRepository.AddTrackerAsync(new Tracker("Test Tracker 3", "Test Tracker", "00:11:22:33:44:55", room), default).GetAwaiter().GetResult();
}


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
