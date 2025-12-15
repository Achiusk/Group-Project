using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    public class P1SensorService : IP1SensorService
    {
   private readonly EnergyDbContext _dbContext;
   private readonly ILogger<P1SensorService> _logger;
        private static readonly string[] EindhovenWijken = new[]
    {
   "Strijp", "Woensel-Noord", "Woensel-Zuid", "Gestel",
     "Stratum", "Tongelre", "Centrum", "Meerhoven"
        };

        public P1SensorService(EnergyDbContext dbContext, ILogger<P1SensorService> logger)
        {
         _dbContext = dbContext;
     _logger = logger;
     }

        public async Task<List<P1SensorData>> GetLatestReadingsAsync()
      {
         try
   {
     var latestReadings = await _dbContext.P1SensorData
     .GroupBy(p => p.ConsumerId)
.Select(g => g.OrderByDescending(p => p.Timestamp).First())
     .ToListAsync();

      if (latestReadings.Any())
    return latestReadings;

                return GenerateMockData();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch P1 sensor data from database, using mock data");
   return GenerateMockData();
   }
        }

    public async Task<P1SensorData?> GetLatestConsumerReadingAsync(string consumerId)
        {
            try
   {
return await _dbContext.P1SensorData
            .Where(p => p.ConsumerId == consumerId)
        .OrderByDescending(p => p.Timestamp)
        .FirstOrDefaultAsync();
    }
            catch (Exception ex)
    {
    _logger.LogWarning(ex, "Failed to fetch consumer {ConsumerId} data", consumerId);
     return null;
    }
        }

    public async Task<List<ConsumerEnergyData>> GetConsumerSummariesAsync()
 {
        try
      {
   var consumers = await _dbContext.Consumers
            .Where(c => c.IsActive)
     .ToListAsync();

          if (!consumers.Any())
        return GenerateMockConsumerSummaries();

       var today = DateTime.UtcNow.Date;
   var monthStart = new DateTime(today.Year, today.Month, 1);
  var summaries = new List<ConsumerEnergyData>();

          foreach (var consumer in consumers)
     {
         var latestReading = await _dbContext.P1SensorData
       .Where(p => p.ConsumerId == consumer.ConsumerId)
          .OrderByDescending(p => p.Timestamp)
         .FirstOrDefaultAsync();

         var todayReadings = await _dbContext.P1SensorData
          .Where(p => p.ConsumerId == consumer.ConsumerId && p.Timestamp >= today)
             .ToListAsync();

       var monthReadings = await _dbContext.P1SensorData
   .Where(p => p.ConsumerId == consumer.ConsumerId && p.Timestamp >= monthStart)
 .ToListAsync();

        summaries.Add(new ConsumerEnergyData
          {
     ConsumerId = consumer.ConsumerId,
DisplayName = consumer.DisplayName,
     Address = consumer.Address,
               Wijk = consumer.Wijk,
      Postcode = consumer.Postcode,
    CurrentPowerKw = latestReading != null ? (double)latestReading.CurrentPowerConsumption : 0,
       CurrentSolarReturnKw = latestReading != null ? (double)latestReading.CurrentPowerReturn : 0,
              TodayConsumptionKwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerConsumption) : 0,
    TodaySolarReturnKwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerReturn) : 0,
            MonthGasConsumptionM3 = monthReadings.Any() ? (double)monthReadings.Max(r => r.GasConsumption) : 0,
            LastUpdate = latestReading?.Timestamp ?? DateTime.UtcNow,
           HasSolarPanels = consumer.HasSolarPanels
     });
       }

     return summaries.Any() ? summaries : GenerateMockConsumerSummaries();
            }
       catch (Exception ex)
        {
       _logger.LogWarning(ex, "Failed to fetch consumer summaries, using mock data");
      return GenerateMockConsumerSummaries();
   }
        }

      public async Task<List<WijkEnergyStatistics>> GetWijkStatisticsAsync()
        {
            try
            {
                var stats = new List<WijkEnergyStatistics>();

    foreach (var wijk in EindhovenWijken)
      {
   var consumers = await _dbContext.Consumers
     .Where(c => c.Wijk == wijk && c.IsActive)
              .ToListAsync();

         if (!consumers.Any())
            {
stats.Add(GenerateMockWijkStat(wijk));
 continue;
           }

     var consumerIds = consumers.Select(c => c.ConsumerId).ToList();

            var latestReadings = await _dbContext.P1SensorData
    .Where(p => consumerIds.Contains(p.ConsumerId))
  .GroupBy(p => p.ConsumerId)
.Select(g => g.OrderByDescending(p => p.Timestamp).First())
        .ToListAsync();

           var today = DateTime.UtcNow.Date;
    var todayReadings = await _dbContext.P1SensorData
        .Where(p => consumerIds.Contains(p.ConsumerId) && p.Timestamp >= today)
         .ToListAsync();

        var monthStart = new DateTime(today.Year, today.Month, 1);
  var monthReadings = await _dbContext.P1SensorData
              .Where(p => consumerIds.Contains(p.ConsumerId) && p.Timestamp >= monthStart)
       .ToListAsync();

        stats.Add(new WijkEnergyStatistics
     {
          WijkName = wijk,
           TotalHouseholds = consumers.Count,
     HouseholdsWithSolar = consumers.Count(c => c.HasSolarPanels),
          TotalCurrentConsumptionKw = latestReadings.Any() ? (double)latestReadings.Sum(r => r.CurrentPowerConsumption) : 0,
       TotalCurrentSolarReturnKw = latestReadings.Any() ? (double)latestReadings.Sum(r => r.CurrentPowerReturn) : 0,
         TodayTotalConsumptionKwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerConsumption) : 0,
               TodayTotalSolarReturnKwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerReturn) : 0,
             MonthTotalGasM3 = monthReadings.Any() ? (double)monthReadings.GroupBy(r => r.ConsumerId).Sum(g => g.Max(r => r.GasConsumption)) : 0,
      AverageConsumptionPerHousehold = consumers.Count > 0 && latestReadings.Any()
                ? (double)latestReadings.Sum(r => r.CurrentPowerConsumption) / consumers.Count : 0,
       LastUpdate = DateTime.UtcNow
             });
        }

         return stats.Any() ? stats : GenerateMockWijkStatistics();
         }
    catch (Exception ex)
          {
          _logger.LogWarning(ex, "Failed to fetch wijk statistics, using mock data");
      return GenerateMockWijkStatistics();
        }
        }

    public async Task<List<P1SensorData>> GetConsumerHistoryAsync(string consumerId, DateTime from, DateTime to)
   {
            try
            {
                return await _dbContext.P1SensorData
          .Where(p => p.ConsumerId == consumerId && p.Timestamp >= from && p.Timestamp <= to)
    .OrderBy(p => p.Timestamp)
    .ToListAsync();
    }
      catch (Exception ex)
    {
    _logger.LogWarning(ex, "Failed to fetch history for {ConsumerId}", consumerId);
            return new List<P1SensorData>();
     }
        }

        public async Task<CityEnergyStatistics> GetCityStatisticsAsync()
        {
            try
            {
           var consumers = await _dbContext.Consumers.Where(c => c.IsActive).ToListAsync();

             if (!consumers.Any())
                    return GenerateMockCityStatistics();

       var consumerIds = consumers.Select(c => c.ConsumerId).ToList();

                var latestReadings = await _dbContext.P1SensorData
    .Where(p => consumerIds.Contains(p.ConsumerId))
     .GroupBy(p => p.ConsumerId)
           .Select(g => g.OrderByDescending(p => p.Timestamp).First())
         .ToListAsync();

          var today = DateTime.UtcNow.Date;
 var todayReadings = await _dbContext.P1SensorData
      .Where(p => consumerIds.Contains(p.ConsumerId) && p.Timestamp >= today)
  .ToListAsync();

            var monthStart = new DateTime(today.Year, today.Month, 1);
     var monthReadings = await _dbContext.P1SensorData
           .Where(p => consumerIds.Contains(p.ConsumerId) && p.Timestamp >= monthStart)
         .ToListAsync();

  return new CityEnergyStatistics
 {
  TotalHouseholds = consumers.Count,
      HouseholdsWithSolar = consumers.Count(c => c.HasSolarPanels),
      TotalCurrentConsumptionMw = latestReadings.Any() ? (double)latestReadings.Sum(r => r.CurrentPowerConsumption) / 1000 : 0,
   TotalCurrentSolarReturnMw = latestReadings.Any() ? (double)latestReadings.Sum(r => r.CurrentPowerReturn) / 1000 : 0,
     TodayTotalConsumptionMwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerConsumption) / 1000 : 0,
       TodayTotalSolarReturnMwh = todayReadings.Any() ? (double)todayReadings.Sum(r => r.CurrentPowerReturn) / 1000 : 0,
 MonthTotalGasM3 = monthReadings.Any() ? (double)monthReadings.GroupBy(r => r.ConsumerId).Sum(g => g.Max(r => r.GasConsumption)) : 0,
        AverageConsumptionPerHousehold = consumers.Count > 0 && latestReadings.Any()
     ? (double)latestReadings.Sum(r => r.CurrentPowerConsumption) / consumers.Count : 0,
   LastUpdate = DateTime.UtcNow
   };
      }
    catch (Exception ex)
            {
   _logger.LogWarning(ex, "Failed to fetch city statistics, using mock data");
             return GenerateMockCityStatistics();
 }
        }

        public async Task<bool> SaveReadingAsync(P1SensorData reading)
        {
      try
        {
    _dbContext.P1SensorData.Add(reading);
     await _dbContext.SaveChangesAsync();
          return true;
            }
            catch (Exception ex)
         {
   _logger.LogError(ex, "Failed to save P1 sensor reading");
            return false;
}
     }

  public async Task<List<Consumer>> GetAllConsumersAsync()
        {
  try
    {
       var consumers = await _dbContext.Consumers
    .Where(c => c.IsActive)
    .OrderBy(c => c.Wijk)
    .ThenBy(c => c.DisplayName)
  .ToListAsync();

             return consumers.Any() ? consumers : GenerateMockConsumers();
            }
          catch (Exception ex)
     {
    _logger.LogWarning(ex, "Failed to fetch consumers, using mock data");
      return GenerateMockConsumers();
    }
        }

        public async Task<List<Consumer>> GetConsumersByWijkAsync(string wijk)
        {
         try
     {
      return await _dbContext.Consumers
  .Where(c => c.Wijk == wijk && c.IsActive)
.OrderBy(c => c.DisplayName)
          .ToListAsync();
         }
        catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch consumers for wijk {Wijk}", wijk);
         return new List<Consumer>();
    }
        }

        private List<P1SensorData> GenerateMockData()
{
        var random = new Random();
            var data = new List<P1SensorData>();

            for (int i = 1; i <= 50; i++)
    {
   var wijk = EindhovenWijken[random.Next(EindhovenWijken.Length)];
       data.Add(new P1SensorData
    {
       MeterId = $"E00{i:D5}",
     ConsumerId = $"CONS-{i:D4}",
         Address = $"{GetRandomStreet(random)} {random.Next(1, 200)}, Eindhoven",
            Postcode = $"56{random.Next(10, 99)} {GetRandomLetters(random)}",
            Wijk = wijk,
       Timestamp = DateTime.UtcNow,
     ElectricityDeliveredTariff1 = (decimal)(random.NextDouble() * 5000 + 1000),
             ElectricityDeliveredTariff2 = (decimal)(random.NextDouble() * 3000 + 500),
              ElectricityReturnedTariff1 = (decimal)(random.NextDouble() * 1500),
           ElectricityReturnedTariff2 = (decimal)(random.NextDouble() * 1000),
  CurrentTariff = random.Next(1, 3),
  CurrentPowerConsumption = (decimal)(random.NextDouble() * 3 + 0.5),
           CurrentPowerReturn = random.Next(0, 10) > 6 ? (decimal)(random.NextDouble() * 2) : 0,
          GasConsumption = (decimal)(random.NextDouble() * 2000 + 500),
 GasTimestamp = DateTime.UtcNow.AddMinutes(-random.Next(0, 60))
          });
   }

     return data;
        }

    private List<Consumer> GenerateMockConsumers()
        {
   var random = new Random(42);
            var consumers = new List<Consumer>();
        int id = 1;

foreach (var wijk in EindhovenWijken)
         {
                for (int i = 0; i < 10; i++)
      {
       consumers.Add(new Consumer
  {
       ConsumerId = $"CONS-{id:D4}",
     DisplayName = $"Huishouden {wijk} #{i + 1}",
            Address = $"{GetRandomStreet(random)} {random.Next(1, 200)}, Eindhoven",
        Wijk = wijk,
     Postcode = $"56{random.Next(10, 99)} {GetRandomLetters(random)}",
          HasSolarPanels = random.Next(100) > 60,
  CreatedAt = DateTime.UtcNow,
       IsActive = true
      });
           id++;
           }
    }

            return consumers;
        }

        private List<ConsumerEnergyData> GenerateMockConsumerSummaries()
        {
 var random = new Random();
            var summaries = new List<ConsumerEnergyData>();

         for (int i = 1; i <= 20; i++)
        {
   var wijk = EindhovenWijken[random.Next(EindhovenWijken.Length)];
          var hasSolar = random.Next(0, 10) > 4;

         summaries.Add(new ConsumerEnergyData
          {
         ConsumerId = $"CONS-{i:D4}",
                 DisplayName = $"Huishouden {i}",
  Address = $"{GetRandomStreet(random)} {random.Next(1, 200)}",
       Wijk = wijk,
        Postcode = $"56{random.Next(10, 99)} {GetRandomLetters(random)}",
            CurrentPowerKw = Math.Round(random.NextDouble() * 3 + 0.3, 2),
          CurrentSolarReturnKw = hasSolar ? Math.Round(random.NextDouble() * 2, 2) : 0,
         TodayConsumptionKwh = Math.Round(random.NextDouble() * 15 + 5, 2),
             TodaySolarReturnKwh = hasSolar ? Math.Round(random.NextDouble() * 8, 2) : 0,
      MonthGasConsumptionM3 = Math.Round(random.NextDouble() * 150 + 50, 2),
         LastUpdate = DateTime.UtcNow.AddMinutes(-random.Next(0, 5)),
           HasSolarPanels = hasSolar
      });
  }

            return summaries;
}

        private WijkEnergyStatistics GenerateMockWijkStat(string wijk)
     {
   var random = new Random();
     return new WijkEnergyStatistics
            {
                WijkName = wijk,
           TotalHouseholds = random.Next(500, 2000),
     HouseholdsWithSolar = random.Next(100, 600),
  TotalCurrentConsumptionKw = Math.Round(random.NextDouble() * 500 + 200, 2),
                TotalCurrentSolarReturnKw = Math.Round(random.NextDouble() * 150 + 50, 2),
         TodayTotalConsumptionKwh = Math.Round(random.NextDouble() * 5000 + 2000, 2),
 TodayTotalSolarReturnKwh = Math.Round(random.NextDouble() * 1500 + 500, 2),
      MonthTotalGasM3 = Math.Round(random.NextDouble() * 50000 + 20000, 2),
         AverageConsumptionPerHousehold = Math.Round(random.NextDouble() * 3 + 1, 2),
   LastUpdate = DateTime.UtcNow
       };
        }

    private List<WijkEnergyStatistics> GenerateMockWijkStatistics()
        {
     return EindhovenWijken.Select(GenerateMockWijkStat).ToList();
        }

        private CityEnergyStatistics GenerateMockCityStatistics()
      {
         var random = new Random();
         return new CityEnergyStatistics
      {
       TotalHouseholds = 95000,
                HouseholdsWithSolar = 28500,
    TotalCurrentConsumptionMw = Math.Round(random.NextDouble() * 50 + 120, 2),
           TotalCurrentSolarReturnMw = Math.Round(random.NextDouble() * 15 + 25, 2),
       TodayTotalConsumptionMwh = Math.Round(random.NextDouble() * 500 + 1500, 2),
                TodayTotalSolarReturnMwh = Math.Round(random.NextDouble() * 200 + 400, 2),
         MonthTotalGasM3 = Math.Round(random.NextDouble() * 5000000 + 10000000, 0),
 AverageConsumptionPerHousehold = 2.4,
      LastUpdate = DateTime.UtcNow
   };
        }

        private static string GetRandomStreet(Random random)
        {
       string[] streets = {
  "Vestdijk", "Stratumseind", "Hoogstraat", "Markt", "Willemstraat",
  "Strijpsestraat", "Woenselsestraat", "Gestelsestraat", "Tongelresestraat",
  "Meerhoven", "Fellenoord", "Keizersgracht", "Prinsenlaan", "Hertogstraat"
       };
         return streets[random.Next(streets.Length)];
        }

        private static string GetRandomLetters(Random random)
        {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ";
    return $"{chars[random.Next(chars.Length)]}{chars[random.Next(chars.Length)]}";
   }
    }
}
