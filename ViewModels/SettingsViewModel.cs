using Avalonia.Styling;
using ReactiveUI;
using System;
using System.Reactive;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
  private readonly ILocalizationService _localizationService;
   private readonly IThemeService _themeService;

 public SettingsViewModel(
      ILocalizationService localizationService,
    IThemeService themeService)
 {
   _localizationService = localizationService;
   _themeService = themeService;

            // Subscribe to service events to update UI when language/theme changes
       _localizationService.LanguageChanged += OnLanguageChanged;
  _themeService.ThemeChanged += OnThemeChanged;

    SetDutchCommand = ReactiveCommand.Create(() => _localizationService.SetLanguage("nl"));
  SetEnglishCommand = ReactiveCommand.Create(() => _localizationService.SetLanguage("en"));
      SetLightThemeCommand = ReactiveCommand.Create(() => _themeService.SetTheme(ThemeVariant.Light));
   SetDarkThemeCommand = ReactiveCommand.Create(() => _themeService.SetTheme(ThemeVariant.Dark));
   }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
          // Notify UI that all localized strings need to refresh
this.RaisePropertyChanged(nameof(Title));
    this.RaisePropertyChanged(nameof(LanguageLabel));
         this.RaisePropertyChanged(nameof(ThemeLabel));
            this.RaisePropertyChanged(nameof(DutchLabel));
       this.RaisePropertyChanged(nameof(EnglishLabel));
    this.RaisePropertyChanged(nameof(LightModeLabel));
            this.RaisePropertyChanged(nameof(DarkModeLabel));
       this.RaisePropertyChanged(nameof(CloudLabel));
    this.RaisePropertyChanged(nameof(AboutLabel));
            this.RaisePropertyChanged(nameof(SecurityLabel));
        }

    private void OnThemeChanged(object? sender, EventArgs e)
        {
            // Theme changed - UI will update automatically via Avalonia's theme system
System.Diagnostics.Debug.WriteLine($"Theme changed to: {_themeService.CurrentTheme}");
      }

   public string Title => _localizationService.GetString("Settings");
        public string LanguageLabel => _localizationService.GetString("Language");
public string ThemeLabel => _localizationService.GetString("Theme");
        public string DutchLabel => _localizationService.GetString("Dutch");
  public string EnglishLabel => _localizationService.GetString("English");
        public string LightModeLabel => _localizationService.GetString("LightMode");
public string DarkModeLabel => _localizationService.GetString("DarkMode");
        public string CloudLabel => _localizationService.GetString("Cloud");
  public string AboutLabel => _localizationService.GetString("About");
public string SecurityLabel => _localizationService.GetString("Security");


  // Public methods for code-behind event handlers
 public void SetLanguage(string language)
   {
  _localizationService.SetLanguage(language);
 }

        public void SetLightTheme()
   {
 _themeService.SetTheme(ThemeVariant.Light);
        }

        public void SetDarkTheme()
   {
  _themeService.SetTheme(ThemeVariant.Dark);
  }

  // Legacy commands - can be removed later
 public ReactiveCommand<Unit, Unit> SetDutchCommand { get; }
        public ReactiveCommand<Unit, Unit> SetEnglishCommand { get; }
 public ReactiveCommand<Unit, Unit> SetLightThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> SetDarkThemeCommand { get; }
    }
}
