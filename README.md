# Urban City Power Management

Energy management system for Eindhoven municipality with real-time monitoring of household energy consumption via P1 smart meters.

## Tech Stack

| Component | Technology |
|-----------|------------|
| **Backend** | .NET 9, Entity Framework Core |
| **Web App** | Blazor Server |
| **Desktop App** | Avalonia UI |
| **Database** | SQL Server / Azure SQL |
| **Cloud** | Azure App Service, Key Vault |

## Quick Start

```bash
# Clone
git clone https://github.com/Achiusk/Group-Project.git
cd "urban city power managment"

# Run Web App
cd urban_city_power_managment.Web
dotnet run
```

**Web App URL:** http://localhost:5022

## Features

- **Dashboard** - Real-time energy overview with efficiency scores
- **Mijn Energie** - P1 smart meter data visualization
- **Bespaartips** - Personalized energy saving recommendations
- **Leveranciers** - Vendor directory for energy services
- **Gas Monitoring** - Leak detection and pressure alerts

## Project Structure

```
??? urban_city_power_managment.Web/    # Blazor Web Application
?   ??? Components/Pages/         # Razor pages
?   ??? Services/# Business logic
?   ??? Models/    # Data models
?   ??? Database/     # SQL scripts
?
??? [root]/                # Avalonia Desktop Application
    ??? Views/# AXAML views
    ??? ViewModels/             # MVVM ViewModels
    ??? Services/    # Desktop services
```

## Database Setup

```bash
# Using LocalDB (development)
sqlcmd -S "(localdb)\mssqllocaldb" -i urban_city_power_managment.Web/Database/setup-database.sql

# Add mock data
sqlcmd -S "(localdb)\mssqllocaldb" -d EindhovenEnergy -i urban_city_power_managment.Web/Database/clients-data.sql
```

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EindhovenEnergy;Trusted_Connection=True"
  }
}
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `/` | Consumer dashboard |
| `/mijn-energie` | Personal energy data |
| `/tips` | Energy saving tips |
| `/leveranciers` | Vendor directory |

## Authors

FHICT Group Project Team

## License

Educational use only - © 2024
