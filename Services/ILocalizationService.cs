using System;

namespace urban_city_power_managment.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        event EventHandler? LanguageChanged;
   void SetLanguage(string languageCode);
        string GetString(string key);
    }
}
