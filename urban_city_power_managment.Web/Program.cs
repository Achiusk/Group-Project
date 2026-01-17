using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add secure session support for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;        // Prevents JavaScript access (XSS protection)
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF protection
    options.Cookie.Name = ".EindhovenEnergie.Session";  // Custom name (security through obscurity)
});

// Add HttpContextAccessor for accessing session in services
builder.Services.AddHttpContextAccessor();

// Add Data Protection for secure cookie encryption
builder.Services.AddDataProtection();

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
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Simple database initialization and seeding
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            await dbContext.Database.EnsureCreatedAsync();
            await seeder.SeedAsync();
            Console.WriteLine("? Database initialized and seeded with mock data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Database initialization skipped: {ex.Message}");
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add session middleware (must be before routing)
app.UseSession();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("? Application starting with secure authentication support...");
app.Run();
