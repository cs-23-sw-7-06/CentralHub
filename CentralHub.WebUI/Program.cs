using CentralHub.WebUI.Data;

namespace CentralHub.WebUI;

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
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton<RoomService>();
        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}