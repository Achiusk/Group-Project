# Eindhoven Energie - Smart Energy Management

A modern Blazor Server application for monitoring residential energy consumption via P1 smart meters.

![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-blue)
![MySQL](https://img.shields.io/badge/Database-MySQL-orange)
![Azure](https://img.shields.io/badge/Deployed-Azure-0078D4)

---

## Features

- **Real-time Dashboard** - Live energy consumption and solar return monitoring
- **Energy Analytics** - Detailed usage statistics with charts and comparisons
- **Saving Tips** - Personalized recommendations based on consumption patterns
- **Supplier Directory** - Find and compare local energy service providers
- **Weather Integration** - Weather forecasts with energy impact analysis
- **Dark Mode** - Full dark theme with green accent styling
- **Bilingual** - Dutch (NL) and English (EN) support

---

## Quick Start

### Prerequisites
- .NET 9 SDK
- MySQL Server 8.0+

### Run Locally
```bash
dotnet run --project urban_city_power_managment.Web
```

### Configuration
Update `appsettings.json` with your MySQL connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EindhovenEnergy;User=your_user;Password=your_password;"
  }
}
```

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | Blazor Server (.NET 9) |
| Database | MySQL + Pomelo EF Core |
| Auth | BCrypt password hashing |
| UI | Bootstrap 5 |
| Weather | Open-Meteo API |
| Hosting | Azure App Service |

---

## Project Structure

```
urban_city_power_managment.Web/
??? Components/
?   ??? Layout/          # MainLayout, NavMenu
?   ??? Pages/           # All application pages
??? Data/                # EF Core DbContext
??? Models/              # Data models
??? Services/            # Business logic
??? wwwroot/             # Static assets
```

---

## Deployment

The application is deployed to Azure App Service with MySQL In-App database.

**Live URL:** [Eindhoven Energie](https://urbancitypowermanagmentweb20260114230645-ejhra4hkghhga3e4.italynorth-01.azurewebsites.net)

---

## Team

Fontys ICT - Group Project Team

---

## License

Educational purposes only.

---

*Gemeente Eindhoven - Stedelijk Energiebeheer*
