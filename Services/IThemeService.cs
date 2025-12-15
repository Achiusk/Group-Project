using Avalonia.Styling;
using System;

namespace urban_city_power_managment.Services
{
    public interface IThemeService
    {
   ThemeVariant CurrentTheme { get; }
        event EventHandler? ThemeChanged;
        void SetTheme(ThemeVariant theme);
        void ToggleTheme();
    }
}
