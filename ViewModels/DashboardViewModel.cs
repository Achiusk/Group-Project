using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class DashboardViewModel : ViewModelBase
 {
   private readonly ILocalizationService _localizationService;

     public DashboardViewModel(ILocalizationService localizationService)
        {
_localizationService = localizationService;
        }

        public string Title => _localizationService.GetString("Dashboard");
    }
}
