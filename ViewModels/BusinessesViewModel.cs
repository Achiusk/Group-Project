using ReactiveUI;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class BusinessesViewModel : ViewModelBase
 {
        private readonly ILocalizationService _localizationService;
  private double _consumption = 5680.2;
        private double _solarReturn = 1240.5;

  public BusinessesViewModel(ILocalizationService localizationService)
      {
  _localizationService = localizationService;
   }

public string Title => _localizationService.GetString("BusinessDashboard");

 public double Consumption
     {
 get => _consumption;
     set => this.RaiseAndSetIfChanged(ref _consumption, value);
   }

        public double SolarReturn
 {
  get => _solarReturn;
     set => this.RaiseAndSetIfChanged(ref _solarReturn, value);
    }

   public double NetConsumption => Consumption - SolarReturn;

        public string ConsumptionLabel => _localizationService.GetString("Consumption");
      public string SolarReturnLabel => _localizationService.GetString("SolarReturn");
  public string NetConsumptionLabel => _localizationService.GetString("NetConsumption");
    }
}
