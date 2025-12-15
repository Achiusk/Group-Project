using System;
using System.Collections.Generic;

namespace urban_city_power_managment.Services
{
    public class LocalizationService : ILocalizationService
    {
        private string _currentLanguage = "nl"; // Default Dutch
      private readonly Dictionary<string, Dictionary<string, string>> _translations;

    public string CurrentLanguage => _currentLanguage;
        public event EventHandler? LanguageChanged;

        public LocalizationService()
        {
   _translations = new Dictionary<string, Dictionary<string, string>>
     {
        ["nl"] = new Dictionary<string, string>
        {
       // Main Menu
         ["AppTitle"] = "Stedelijk Energiebeheer Eindhoven",
    ["Dashboard"] = "Dashboard",
     ["PowerGeneration"] = "Energieopwekking",
          ["PowerConsumption"] = "Energieverbruik",
   ["Consumers"] = "Consumenten",
     ["Businesses"] = "Bedrijven",
         ["GasInfrastructure"] = "Gasinfrastructuur",
    ["Notifications"] = "Meldingen",
 ["Tasks"] = "Taken",
       ["Settings"] = "Instellingen",
        
   // Power Generation
      ["Wind"] = "Wind",
   ["Water"] = "Water",
    ["Coal"] = "Kolen",
   ["TotalGeneration"] = "Totale Opwekking",
     ["GenerationOverview"] = "Opwekkingsoverzicht",
        
         // Power Consumption
  ["Consumption"] = "Verbruik",
              ["SolarReturn"] = "Zonnepanelen Teruglevering",
      ["NetConsumption"] = "Netto Verbruik",
     ["ConsumerDashboard"] = "Consumentendashboard",
  ["BusinessDashboard"] = "Bedrijvendashboard",
        
      // Gas Infrastructure
      ["GasUsage"] = "Gasverbruik",
   ["GasLeaks"] = "Gaslekken",
            ["Pressure"] = "Druk",
       ["FlowRate"] = "Stroomsnelheid",
    ["GasTemperature"] = "Gastemperatuur",
   ["TotalGasConsumption"] = "Totaal Gasverbruik",
        ["ActiveAlerts"] = "Actieve Waarschuwingen",
  ["LeakDetected"] = "Lek Gedetecteerd",
            ["PressureDrop"] = "Drukval",
            ["AffectedCustomers"] = "Getroffen Klanten",
["Severity"] = "Ernst",
       ["SeverityLow"] = "Laag",
      ["SeverityMedium"] = "Gemiddeld",
            ["SeverityHigh"] = "Hoog",
            ["SeverityCritical"] = "Kritiek",
      ["ResolveAlert"] = "Waarschuwing Oplossen",
            ["GasMonitoring"] = "Gasmonitoring",
  ["ZoneStatus"] = "Zonestatus",
            
         // Weather
         ["Weather"] = "Weer",
       ["Temperature"] = "Temperatuur",
       ["Location"] = "Locatie",
       ["Eindhoven"] = "Eindhoven",
       
      // Notifications
        ["Incident"] = "Incident",
    ["UnusualUsage"] = "Ongewoon Verbruik",
     ["NoNotifications"] = "Geen nieuwe meldingen",
            ["GasLeak"] = "Gaslek",
          
    // Settings
     ["Language"] = "Taal",
  ["Theme"] = "Thema",
  ["LightMode"] = "Lichte Modus",
      ["DarkMode"] = "Donkere Modus",
    ["Dutch"] = "Nederlands",
        ["English"] = "Engels",
  ["Cloud"] = "Cloud",
            ["About"] = "Over",
      ["Security"] = "Beveiliging",
       
  // Time
  ["Date"] = "Datum",
      ["Time"] = "Tijd",
  
  // Units
         ["MW"] = "MW",
  ["kWh"] = "kWh",
         ["Percentage"] = "",
        ["Bar"] = "bar",
            ["CubicMetersPerHour"] = "m³/uur",
            ["CubicMeters"] = "m³",
 ["Celsius"] = "°C"
     },
    ["en"] = new Dictionary<string, string>
     {
   // Main Menu
        ["AppTitle"] = "Urban Energy Management Eindhoven",
        ["Dashboard"] = "Dashboard",
           ["PowerGeneration"] = "Power Generation",
   ["PowerConsumption"] = "Power Consumption",
           ["Consumers"] = "Consumers",
        ["Businesses"] = "Businesses",
         ["GasInfrastructure"] = "Gas Infrastructure",
         ["Notifications"] = "Notifications",
       ["Tasks"] = "Tasks",
          ["Settings"] = "Settings",
    
   // Power Generation
        ["Wind"] = "Wind",
      ["Water"] = "Water",
        ["Coal"] = "Coal",
           ["TotalGeneration"] = "Total Generation",
     ["GenerationOverview"] = "Generation Overview",
  
             // Power Consumption
           ["Consumption"] = "Consumption",
    ["SolarReturn"] = "Solar Panel Return",
       ["NetConsumption"] = "Net Consumption",
         ["ConsumerDashboard"] = "Consumer Dashboard",
             ["BusinessDashboard"] = "Business Dashboard",
     
      // Gas Infrastructure
   ["GasUsage"] = "Gas Usage",
          ["GasLeaks"] = "Gas Leaks",
      ["Pressure"] = "Pressure",
         ["FlowRate"] = "Flow Rate",
     ["GasTemperature"] = "Gas Temperature",
            ["TotalGasConsumption"] = "Total Gas Consumption",
   ["ActiveAlerts"] = "Active Alerts",
            ["LeakDetected"] = "Leak Detected",
   ["PressureDrop"] = "Pressure Drop",
     ["AffectedCustomers"] = "Affected Customers",
 ["Severity"] = "Severity",
      ["SeverityLow"] = "Low",
      ["SeverityMedium"] = "Medium",
            ["SeverityHigh"] = "High",
["SeverityCritical"] = "Critical",
            ["ResolveAlert"] = "Resolve Alert",
      ["GasMonitoring"] = "Gas Monitoring",
            ["ZoneStatus"] = "Zone Status",
            
        // Weather
 ["Weather"] = "Weather",
         ["Temperature"] = "Temperature",
    ["Location"] = "Location",
    ["Eindhoven"] = "Eindhoven",
  
    // Notifications
      ["Incident"] = "Incident",
       ["UnusualUsage"] = "Unusual Usage",
   ["NoNotifications"] = "No new notifications",
          ["GasLeak"] = "Gas Leak",
       
   // Settings
["Language"] = "Language",
         ["Theme"] = "Theme",
    ["LightMode"] = "Light Mode",
["DarkMode"] = "Dark Mode",
  ["Dutch"] = "Dutch",
       ["English"] = "English",
 ["Cloud"] = "Cloud",
      ["About"] = "About",
["Security"] = "Security",
        
         // Time
              ["Date"] = "Date",
    ["Time"] = "Time",
    
      // Units
        ["MW"] = "MW",
        ["kWh"] = "kWh",
  ["Percentage"] = "",
       ["Bar"] = "bar",
   ["CubicMetersPerHour"] = "m³/h",
        ["CubicMeters"] = "m³",
            ["Celsius"] = "°C"
   }
            };
     }

        public void SetLanguage(string languageCode)
   {
       System.Diagnostics.Debug.WriteLine($"LocalizationService.SetLanguage called with: {languageCode}");
       if (_translations.ContainsKey(languageCode))
 {
  _currentLanguage = languageCode;
       System.Diagnostics.Debug.WriteLine($"Language changed to: {_currentLanguage}");
                System.Diagnostics.Debug.WriteLine($"Firing LanguageChanged event...");
   LanguageChanged?.Invoke(this, EventArgs.Empty);
    System.Diagnostics.Debug.WriteLine($"LanguageChanged event fired. Subscriber count: {LanguageChanged?.GetInvocationList().Length ?? 0}");
 }
else
   {
              System.Diagnostics.Debug.WriteLine($"Language code '{languageCode}' not found in translations!");
  }
        }

  public string GetString(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var languageDict) &&
   languageDict.TryGetValue(key, out var value))
            {
        return value;
            }
     return key; // Return key if translation not found
 }
    }
}
