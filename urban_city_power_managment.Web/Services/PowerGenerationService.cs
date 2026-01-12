using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    public interface IPowerGenerationService
    {
        Task<PowerGeneration> GetCurrentGenerationAsync();
        Task<CityPowerStatistics> GetCityStatisticsAsync();
    }

    public class PowerGenerationService : IPowerGenerationService
    {
      private readonly Random _random = new();

        public Task<PowerGeneration> GetCurrentGenerationAsync()
        {
         var windPower = Math.Round(_random.NextDouble() * 15 + 5, 2);
    var waterPower = Math.Round(_random.NextDouble() * 3 + 2, 2);
         var solarPower = GetSolarPower();
   var coalPower = Math.Round(_random.NextDouble() * 30 + 40, 2);
         
            var total = windPower + waterPower + solarPower + coalPower;

var generation = new PowerGeneration
   {
        WindPowerMw = windPower,
                WindPercentage = Math.Round(windPower / total * 100, 1),
  WaterPowerMw = waterPower,
        WaterPercentage = Math.Round(waterPower / total * 100, 1),
            SolarPowerMw = solarPower,
                SolarPercentage = Math.Round(solarPower / total * 100, 1),
   CoalPowerMw = coalPower,
  CoalPercentage = Math.Round(coalPower / total * 100, 1),
    TotalGenerationMw = Math.Round(total, 2),
        Timestamp = DateTime.UtcNow
         };

    return Task.FromResult(generation);
        }

  public Task<CityPowerStatistics> GetCityStatisticsAsync()
    {
            var hour = DateTime.Now.Hour;
       
            var baseDemand = hour switch
   {
                >= 6 and < 9 => 85,
     >= 9 and < 17 => 95,
   >= 17 and < 21 => 100,
                >= 21 and < 23 => 80,
    _ => 60
   };
            
     var demand = Math.Round(baseDemand + _random.NextDouble() * 10 - 5, 2);
    var supply = Math.Round(demand + _random.NextDouble() * 15 - 7.5, 2);
            var renewablePercent = Math.Round(20 + _random.NextDouble() * 15, 1);
 var co2Saved = Math.Round(renewablePercent * 0.45, 2);

          return Task.FromResult(new CityPowerStatistics
            {
   TotalDemandMw = demand,
          TotalSupplyMw = supply,
      RenewablePercentage = renewablePercent,
       CO2SavedTonnes = co2Saved,
                LastUpdated = DateTime.UtcNow
        });
        }

   private double GetSolarPower()
        {
      var hour = DateTime.Now.Hour;
 return hour switch
    {
      >= 6 and < 8 => Math.Round(_random.NextDouble() * 2 + 1, 2),
          >= 8 and < 10 => Math.Round(_random.NextDouble() * 5 + 5, 2),
      >= 10 and < 14 => Math.Round(_random.NextDouble() * 8 + 12, 2),
        >= 14 and < 16 => Math.Round(_random.NextDouble() * 6 + 8, 2),
       >= 16 and < 18 => Math.Round(_random.NextDouble() * 4 + 3, 2),
>= 18 and < 20 => Math.Round(_random.NextDouble() * 2 + 0.5, 2),
        _ => 0
      };
        }
    }
}