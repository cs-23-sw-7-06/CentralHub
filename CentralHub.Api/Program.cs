using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkSqlite();

var id = $"{Guid.NewGuid().ToString()}.db";

var sqliteStringBuilder = new SqliteConnectionStringBuilder()
{
    DataSource = id,
    Mode = SqliteOpenMode.Memory,
    Cache = SqliteCacheMode.Shared
};

builder.Services.AddDbContext<DevicesContext>(options => options.UseSqlite(sqliteStringBuilder.ToString()), ServiceLifetime.Singleton);
builder.Services.AddSingleton<IDevicesRepository, DevicesRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Insert dummy devices
    var devicesRepository = app.Services.GetRequiredService<IDevicesRepository>();
    devicesRepository.AddDevice(new Device("Test Device 1", "AA:BB:CC:DD:EE:FF", DeviceType.WiFi));
    devicesRepository.AddDevice(new Device("Test Device 2", "FF:EE:DD:CC:BB:AA", DeviceType.WiFi));
    devicesRepository.AddDevice(new Device("Test Device 3", "00:11:22:33:44:55", DeviceType.Bluetooth));
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();