using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using urban_city_power_managment.Web.Components;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication state provider for Blazor
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddCascadingAuthenticationState();

// Add secure session support for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;        // Prevents JavaScript access (XSS protection)
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // Works on both HTTP and HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;  // Allow navigation from external sites
    options.Cookie.Name = ".EindhovenEnergie.Session";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();

// Get MySQL connection string from Azure MySQL In-App
var connectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");

// Log connection string status
Console.WriteLine($"?? MYSQLCONNSTR_localdb present: {!string.IsNullOrEmpty(connectionString)}");

// Parse Azure MySQL In-App connection string format
// Azure format: Database=localdb;Data Source=127.0.0.1:port;User Id=user;Password=pass
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("?? Parsing Azure MySQL connection string...");
    Console.WriteLine($"   Raw connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
    
    try
    {
        // Parse Azure MySQL In-App format and convert to Pomelo MySQL format
        var parts = connectionString.Split(';')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToDictionary(
                p => p.Split('=')[0].Trim(),
                p => p.Contains('=') ? string.Join("=", p.Split('=').Skip(1)).Trim() : "",
                StringComparer.OrdinalIgnoreCase
            );
        
        string server = "127.0.0.1";
        string port = "3306";
        string database = "localdb";
        string userId = "";
        string password = "";
        
        // Extract database name
        if (parts.TryGetValue("Database", out var db))
            database = db;
        
        // Extract Data Source (contains host:port)
        if (parts.TryGetValue("Data Source", out var dataSource))
        {
            if (dataSource.Contains(':'))
            {
                var hostPort = dataSource.Split(':');
                server = hostPort[0];
                port = hostPort[1];
            }
            else
            {
                server = dataSource;
            }
        }
        
        // Extract User Id
        if (parts.TryGetValue("User Id", out var user))
            userId = user;
        else if (parts.TryGetValue("UserId", out user))
            userId = user;
        else if (parts.TryGetValue("User", out user))
            userId = user;
        
        // Extract Password
        if (parts.TryGetValue("Password", out var pwd))
            password = pwd;
        
        // Build standard MySQL connection string for Pomelo
        connectionString = $"Server={server};Port={port};Database={database};User={userId};Password={password};";
        
        Console.WriteLine($"   Parsed - Server: {server}, Port: {port}, Database: {database}, User: {userId}");
        Console.WriteLine("? Connection string parsed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error parsing connection string: {ex.Message}");
        // Keep original connection string if parsing fails
    }
}

// Development fallback
if (string.IsNullOrEmpty(connectionString) && builder.Environment.IsDevelopment())
{
    connectionString = "Server=localhost;Port=3306;Database=EindhovenEnergy_Dev;User=root;Password=;";
    Console.WriteLine("?? Using development database connection");
}

// Always register DbContext (even without connection string, for DI purposes)
var serverVersion = new MySqlServerVersion(new Version(5, 7, 9));

if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine($"? Database connection string configured");
    
    builder.Services.AddDbContext<EnergyDbContext>(options =>
    {
        options.UseMySql(connectionString, serverVersion, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            mySqlOptions.CommandTimeout(60);
        });
        
        // Enable detailed errors in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }
    });
}
else
{
    // Register a placeholder DbContext that will fail gracefully
    Console.WriteLine("?? No database connection string - using in-memory fallback");
    builder.Services.AddDbContext<EnergyDbContext>(options =>
    {
        options.UseInMemoryDatabase("EindhovenEnergyFallback");
    });
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

// Register global app state service for theme and language
builder.Services.AddScoped<AppStateService>();

builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Database initialization and seeding
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    
    Console.WriteLine("?? Testing database connection...");
    var canConnect = await dbContext.Database.CanConnectAsync();
    Console.WriteLine($"?? Database connection result: {canConnect}");
    
    if (canConnect)
    {
        Console.WriteLine("?? Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();
        
        Console.WriteLine("?? Running database seeder...");
        await seeder.SeedAsync();
        
        Console.WriteLine("? Database initialized and seeded successfully!");
    }
    else
    {
        Console.WriteLine("?? Cannot connect to database - seeding skipped");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"? Database initialization error: {ex.Message}");
    Console.WriteLine($"   Stack: {ex.StackTrace?.Split('\n').FirstOrDefault()}");
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
app.UseSession();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("?? Application starting...");
app.Run();
