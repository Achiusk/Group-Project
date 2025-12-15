using ReactiveUI;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class ConsumersViewModel : ViewModelBase
    {
 private readonly ILocalizationService _localizationService;
    private double _consumption = 1250.5;
        private double _solarReturn = 320.8;

   public ConsumersViewModel(ILocalizationService localizationService)
        {
     _localizationService = localizationService;
 }

  public string Title => _localizationService.GetString("ConsumerDashboard");
   
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
