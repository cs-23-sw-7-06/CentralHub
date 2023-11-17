using CentralHub.Api.DbContexts;
using CentralHub.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api;

internal static class Program
{
    private static void Main(string[] args)
    {
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

        var dbPath = Path.Combine(stateDirectory, "data.db");

#if DEBUG
        try
        {
            File.Delete(dbPath);
        }
        catch
        {
            // Ignore
        }
#endif
        var connectionString = $"Data Source={dbPath}";

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}