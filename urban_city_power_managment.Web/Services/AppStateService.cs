namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Global application state service for managing theme and language preferences.
    /// This service notifies all subscribed components when settings change.
    /// </summary>
    public class AppStateService
    {
        private bool _isDarkMode = true;
        private bool _isEnglish = false;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool IsEnglish
        {
            get => _isEnglish;
            set
            {
                if (_isEnglish != value)
                {
                    _isEnglish = value;
                    NotifyStateChanged();
                }
            }
        }

        public string CurrentLanguage => IsEnglish ? "en" : "nl";
        public string CurrentTheme => IsDarkMode ? "dark" : "light";

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        // Helper method to get localized text
        public string GetText(string dutch, string english) => IsEnglish ? english : dutch;
    }
}
