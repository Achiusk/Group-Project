# Mijn Energie - Consumer Energy Advisor

A Blazor Server web application for Dutch consumers to monitor their energy consumption through smart meters (P1 sensors).

![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-blue)
![MySQL](https://img.shields.io/badge/Database-MySQL-orange)

## Features

### Consumer Features
- **Dashboard** - Overview of energy consumption and solar return
- **Mijn Energie** - Detailed energy usage statistics
- **Bespaartips** - Personalized energy saving tips
- **Leveranciers** - Energy supplier directory and comparison
- **Weer** - Weather information with energy impact analysis
- **Instellingen** - User settings with dark mode and language switch (NL/EN)

### User Management
- User registration with validation:
  - Names: A-Z letters only (no numbers or aliases)
  - Age: Must be 18+ years old
  - Dutch postal code lookup with automatic address fill
  - Smart meter detection with Netbeheerder contact info
  - Secure password with BCrypt hashing
- User login with remember me option

### Technical Features
- MySQL database integration (Pomelo Entity Framework)
- Dutch address lookup by postal code + house number
- Automatic Netbeheerder detection (Enexis, Liander, Stedin, etc.)
- Real-time weather data from Open-Meteo API
- Dark mode with localStorage persistence
- Dutch/English language switching

## Tech Stack

- **Framework**: Blazor Server (.NET 9)
- **Database**: MySQL with Pomelo EF Core 8.0
- **Authentication**: BCrypt password hashing
- **Styling**: Bootstrap 5
- **Weather API**: Open-Meteo (free, no API key required)

## Project Structure

```
urban_city_power_managment.Web/
??? Components/
?   ??? Layout/
?   ?   ??? NavMenu.razor
?   ??? Pages/
?       ??? Home.razor (Dashboard)
?       ??? MijnEnergie.razor
?       ??? Tips.razor
?       ??? Leveranciers.razor
?       ??? Weather.razor
?       ??? Instellingen.razor
?       ??? Registreren.razor
?       ??? Inloggen.razor
??? Data/
?   ??? EnergyDbContext.cs
??? Models/
?   ??? EnergyModels.cs
?   ??? UserModels.cs
??? Services/
?   ??? AuthService.cs
?   ??? NetbeheerderService.cs
?   ??? PostalCodeService.cs
?   ??? P1SensorService.cs
?   ??? VendorService.cs
?   ??? EnergyTipsService.cs
?   ??? PowerGenerationService.cs
? ??? OpenMeteoWeatherService.cs
??? Program.cs
```

## Getting Started

### Prerequisites
- .NET 9 SDK
- MySQL Server 8.0+

### Configuration

1. Update `appsettings.json` with your MySQL connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=EindhovenEnergy;User=root;Password=your_password;"
  }
}
```

2. Run migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Run the application:
```bash
dotnet run --project urban_city_power_managment.Web
```

## Dutch Grid Operators (Netbeheerders)

The app automatically detects the user's grid operator based on postal code:

| Operator | Regions |
|----------|---------|
| Enexis | Noord-Brabant, Limburg, Groningen, Drenthe |
| Liander | Noord-Holland, Gelderland, Flevoland |
| Stedin | Zuid-Holland, Utrecht |
| Westland Infra | Westland |
| Coteq | Twente |
| Rendo | Parts of Drenthe/Overijssel |

## Screenshots

### Dashboard
Consumer energy overview with real-time consumption data.

### Registration
4-step wizard: Personal info ? Address ? Smart meter ? Password

### Settings
Dark mode toggle and Dutch/English language switch.

## Authors

- Group Project Team - Fontys ICT

## License

This project is for educational purposes.

---

*Gemeente Eindhoven - Stedelijk Energiebeheer*
