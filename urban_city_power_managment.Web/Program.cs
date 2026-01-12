using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Health checks for Docker
builder.Services.AddHealthChecks();

// MySQL Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=EindhovenEnergy;User=root;Password=;";

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
    });
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
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("MySQL Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "MySQL Database initialization failed. Using mock data.");
}
}

// Health check endpoint for Docker
app.MapHealthChecks("/health");

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
