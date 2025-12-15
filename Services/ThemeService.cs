using Avalonia;
using Avalonia.Styling;
using System;

namespace urban_city_power_managment.Services
{
    public class ThemeService : IThemeService
    {
      private ThemeVariant _currentTheme = ThemeVariant.Light;

        public ThemeVariant CurrentTheme => _currentTheme;
        public event EventHandler? ThemeChanged;

        public void SetTheme(ThemeVariant theme)
        {
   System.Diagnostics.Debug.WriteLine($"===== ThemeService.SetTheme START =====");
     System.Diagnostics.Debug.WriteLine($"Requested theme: {theme}");
    System.Diagnostics.Debug.WriteLine($"Application.Current is null: {Application.Current == null}");
      
            _currentTheme = theme;
            if (Application.Current != null)
       {
      System.Diagnostics.Debug.WriteLine($"BEFORE: Application.Current.RequestedThemeVariant = {Application.Current.RequestedThemeVariant}");
  Application.Current.RequestedThemeVariant = theme;
       System.Diagnostics.Debug.WriteLine($"AFTER: Application.Current.RequestedThemeVariant = {Application.Current.RequestedThemeVariant}");
   System.Diagnostics.Debug.WriteLine($"Theme applied successfully");
   }
   else
            {
     System.Diagnostics.Debug.WriteLine("ERROR: Application.Current is NULL - cannot set theme!");
 }
   
   System.Diagnostics.Debug.WriteLine($"Firing ThemeChanged event...");
          ThemeChanged?.Invoke(this, EventArgs.Empty);
System.Diagnostics.Debug.WriteLine($"ThemeChanged event fired. Subscriber count: {ThemeChanged?.GetInvocationList().Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"===== ThemeService.SetTheme END =====");
      }

        public void ToggleTheme()
        {
        var newTheme = _currentTheme == ThemeVariant.Light 
      ? ThemeVariant.Dark 
       : ThemeVariant.Light;
            SetTheme(newTheme);
        }
    }
}
