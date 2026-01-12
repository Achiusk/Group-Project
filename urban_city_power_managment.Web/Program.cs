using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Health checks for Docker and Azure
builder.Services.AddHealthChecks();

// Azure Application Insights (if configured)
var appInsightsConnectionString = builder.Configuration["Azure:ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
      options.ConnectionString = appInsightsConnectionString;
    });
}

// MySQL Connection String - works with Azure MySQL, Docker MySQL, or local MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
 ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=EindhovenEnergy;User=root;Password=;";

// Handle Azure MySQL connection string format
if (connectionString.Contains("Database=") == false && connectionString.Contains("localhost") == false)
{
    // Azure MySQL flexible server format adjustment if needed
    connectionString = connectionString.Replace("localdb", "localhost");
}

// Configure MySQL with Pomelo provider
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<EnergyDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
   maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Azure recommended settings
        mySqlOptions.CommandTimeout(60);
    });
    
    // Enable sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    }
});

// Authentication & User services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INetbeheerderService, NetbeheerderService>();
builder.Services.AddScoped<IPostalCodeService, PostalCodeService>();

// Consumer-focused services
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

// Initialize MySQL database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply pending migrations in production, create in development
        if (app.Environment.IsProduction())
   {
            await dbContext.Database.MigrateAsync();
  logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
      await dbContext.Database.EnsureCreatedAsync();
  logger.LogInformation("Database created/verified successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database initialization failed. App will continue with limited functionality.");
    }
}

// Health check endpoint for Docker and Azure
app.MapHealthChecks("/health");

// Azure warmup endpoint
app.MapGet("/warmup", () => Results.Ok("Warmed up!"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
