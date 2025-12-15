using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=EindhovenEnergy;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<EnergyDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
      sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 5,
      maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

builder.Services.AddScoped<IP1SensorService, P1SensorService>();
builder.Services.AddScoped<IGasMonitoringService, GasMonitoringService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IEnergyTipsService, EnergyTipsService>();

builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database initialization failed. Using mock data.");
    }
}

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
