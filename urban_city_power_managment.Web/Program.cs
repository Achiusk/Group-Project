using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ONLY Azure MySQL In-App - NO Docker, NO local MySQL complexity
var connectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");

// Development fallback ONLY
if (string.IsNullOrEmpty(connectionString) && builder.Environment.IsDevelopment())
{
    connectionString = "Server=localhost;Port=3306;Database=EindhovenEnergy_Dev;User=root;Password=;";
}

// Configure database ONLY if we have a connection string
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        // Azure MySQL In-App uses MySQL 5.7.9
        var serverVersion = new MySqlServerVersion(new Version(5, 7, 9));
        
        builder.Services.AddDbContext<EnergyDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                mySqlOptions.CommandTimeout(30);
            });
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database configuration failed: {ex.Message}. App will start without database.");
    }
}

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INetbeheerderService, NetbeheerderService>();
builder.Services.AddScoped<IPostalCodeService, PostalCodeService>();
builder.Services.AddScoped<IP1SensorService, P1SensorService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IEnergyTipsService, EnergyTipsService>();
builder.Services.AddScoped<IPowerGenerationService, PowerGenerationService>();

builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Simple database initialization - wrapped to NEVER crash the app
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine("? Database initialized");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Database initialization skipped: {ex.Message}");
        // App continues - pages will work, just no database functionality
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("? Application starting...");
app.Run();
