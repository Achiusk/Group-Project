using ReactiveUI;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class PowerGenerationViewModel : ViewModelBase
    {
  private readonly ILocalizationService _localizationService;
        private double _windPercentage = 20;
      private double _waterPercentage = 20;
     private double _coalPercentage = 60;

        public PowerGenerationViewModel(ILocalizationService localizationService)
        {
   _localizationService = localizationService;
   }

   public string Title => _localizationService.GetString("PowerGeneration");
        public string GenerationOverview => _localizationService.GetString("GenerationOverview");

        public double WindPercentage
   {
     get => _windPercentage;
       set => this.RaiseAndSetIfChanged(ref _windPercentage, value);
        }

      public double WaterPercentage
      {
    get => _waterPercentage;
   set => this.RaiseAndSetIfChanged(ref _waterPercentage, value);
        }

        public double CoalPercentage
        {
     get => _coalPercentage;
       set => this.RaiseAndSetIfChanged(ref _coalPercentage, value);
     }

public string Wind => _localizationService.GetString("Wind");
        public string Water => _localizationService.GetString("Water");
    public string Coal => _localizationService.GetString("Coal");
    }
}
