using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Seeds the database with 10 mock user accounts and realistic P1 sensor data
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly EnergyDbContext _context;
        private readonly Random _random;

        public DatabaseSeeder(EnergyDbContext context)
        {
            _context = context;
            _random = new Random(42); // Fixed seed for reproducible data
        }

        public async Task SeedAsync()
        {
            // Check if we already have users
            if (await _context.Users.AnyAsync())
            {
                Console.WriteLine("Database already seeded - skipping");
                return;
            }

            Console.WriteLine("Seeding database with 10 mock accounts...");

            // Create 10 mock user accounts with dates of birth
            var mockUsers = new []
            {
                new { FirstName = "Jan", LastName = "de Vries", Email = "jan.devries@example.nl", PostalCode = "5611AB", Street = "Vestdijk", Number = "12", Wijk = "Centrum", BirthYear = 1985 },
                new { FirstName = "Emma", LastName = "Jansen", Email = "emma.jansen@example.nl", PostalCode = "5612BC", Street = "Stratumseind", Number = "45", Wijk = "Stratum", BirthYear = 1990 },
                new { FirstName = "Pieter", LastName = "Bakker", Email = "pieter.bakker@example.nl", PostalCode = "5613CD", Street = "Markt", Number = "7", Wijk = "Centrum", BirthYear = 1978 },
                new { FirstName = "Sophie", LastName = "Visser", Email = "sophie.visser@example.nl", PostalCode = "5614DE", Street = "Hoogstraat", Number = "89", Wijk = "Gestel", BirthYear = 1992 },
                new { FirstName = "Lucas", LastName = "Smit", Email = "lucas.smit@example.nl", PostalCode = "5615EF", Street = "Willemstraat", Number = "23", Wijk = "Strijp", BirthYear = 1988 },
                new { FirstName = "Fleur", LastName = "Mulder", Email = "fleur.mulder@example.nl", PostalCode = "5616FG", Street = "Strijpsestraat", Number = "56", Wijk = "Strijp", BirthYear = 1995 },
                new { FirstName = "Daan", LastName = "Bos", Email = "daan.bos@example.nl", PostalCode = "5617GH", Street = "Woenselsestraat", Number = "34", Wijk = "Woensel-Noord", BirthYear = 1982 },
                new { FirstName = "Lisa", LastName = "Meijer", Email = "lisa.meijer@example.nl", PostalCode = "5618HI", Street = "Fellenoord", Number = "78", Wijk = "Woensel-Zuid", BirthYear = 1993 },
                new { FirstName = "Thomas", LastName = "Koning", Email = "thomas.koning@example.nl", PostalCode = "5619IJ", Street = "Veldhovenweg", Number = "15", Wijk = "Meerhoven", BirthYear = 1975 },
                new { FirstName = "Anna", LastName = "Dekker", Email = "anna.dekker@example.nl", PostalCode = "5621KL", Street = "Gentelstraat", Number = "92", Wijk = "Tongelre", BirthYear = 1998 }
            };

            int userId = 1;
            var consumers = new List<Consumer>();

            foreach (var mockUser in mockUsers)
            {
                bool hasSolar = _random.Next(100) > 50;

                // Create user account with all required fields including DateOfBirth
                var user = new UserAccount
                {
                    Id = userId,
                    FirstName = mockUser.FirstName,
                    LastName = mockUser.LastName,
                    Email = mockUser.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 12), // Work factor 12 to match AuthService
                    DateOfBirth = new DateTime(mockUser.BirthYear, _random.Next(1, 12), _random.Next(1, 28)), // Random birth date
                    PostalCode = mockUser.PostalCode,
                    HouseNumber = mockUser.Number,
                    HouseNumberAddition = "",
                    Street = mockUser.Street,
                    City = "Eindhoven",
                    HasSmartMeter = hasSolar,
                    NetbeheerderId = "enexis",
                    NetbeheerderName = "Enexis Netbeheer",
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30, 365)),
                    LastLoginAt = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                    IsActive = true,
                    EmailVerified = true
                };

                _context.Users.Add(user);

                // Create corresponding Consumer
                var consumer = new Consumer
                {
                    ConsumerId = $"USER-{userId:D4}",
                    DisplayName = $"{mockUser.FirstName} {mockUser.LastName}",
                    Address = $"{mockUser.Street} {mockUser.Number}, Eindhoven",
                    Wijk = mockUser.Wijk,
                    Postcode = mockUser.PostalCode,
                    HasSolarPanels = hasSolar,
                    CreatedAt = user.CreatedAt,
                    IsActive = true
                };

                consumers.Add(consumer);
                _context.Consumers.Add(consumer);

                userId++;
            }

            await _context.SaveChangesAsync();

            // Now seed P1 sensor data
            Console.WriteLine("Generating P1 sensor data for the last 7 days...");
            await SeedP1SensorDataAsync(consumers);

            Console.WriteLine("? Database seeded successfully!");
            Console.WriteLine("?? 10 mock accounts created (password: Password123!)");
            Console.WriteLine("?? Test account: jan.devries@example.nl");
        }

        private async Task SeedP1SensorDataAsync(List<Consumer> consumers)
        {
            var today = DateTime.UtcNow.Date;

            foreach (var consumer in consumers)
            {
                // Generate 7 days of hourly P1 sensor data (reduced for faster seeding)
                for (int day = 0; day < 7; day++)
                {
                    var date = today.AddDays(-day);

                    for (int hour = 0; hour < 24; hour++)
                    {
                        var timestamp = date.AddHours(hour);

                        // Generate realistic energy consumption patterns
                        var consumption = GenerateRealisticConsumption(hour, consumer.HasSolarPanels);

                        var sensorData = new P1SensorData
                        {
                            Id = Guid.NewGuid().ToString(),
                            MeterId = $"METER-{consumer.ConsumerId}",
                            ConsumerId = consumer.ConsumerId,
                            Timestamp = timestamp,
                            Address = consumer.Address,
                            Postcode = consumer.Postcode,
                            Wijk = consumer.Wijk,
                            ElectricityDeliveredTariff1 = consumption.DeliveredT1,
                            ElectricityDeliveredTariff2 = consumption.DeliveredT2,
                            ElectricityReturnedTariff1 = consumption.ReturnedT1,
                            ElectricityReturnedTariff2 = consumption.ReturnedT2,
                            CurrentTariff = hour >= 21 || hour < 7 ? 1 : 2,
                            CurrentPowerConsumption = consumption.CurrentPower,
                            CurrentPowerReturn = consumption.CurrentReturn,
                            GasConsumption = GenerateGasConsumption(hour, date.Month)
                        };

                        _context.P1SensorData.Add(sensorData);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private (decimal DeliveredT1, decimal DeliveredT2, decimal ReturnedT1, decimal ReturnedT2, decimal CurrentPower, decimal CurrentReturn) 
            GenerateRealisticConsumption(int hour, bool hasSolar)
        {
            // Realistic hourly consumption patterns
            decimal baseUsage;

            if (hour >= 7 && hour <= 9) // Morning peak
                baseUsage = 1.5m + (decimal)(_random.NextDouble() * 1.0);
            else if (hour >= 17 && hour <= 21) // Evening peak
                baseUsage = 2.0m + (decimal)(_random.NextDouble() * 1.5);
            else if (hour >= 23 || hour <= 6) // Night
                baseUsage = 0.3m + (decimal)(_random.NextDouble() * 0.2);
            else // Rest of day
                baseUsage = 0.8m + (decimal)(_random.NextDouble() * 0.5);

            decimal deliveredT1 = 0;
            decimal deliveredT2 = 0;
            decimal returnedT1 = 0;
            decimal returnedT2 = 0;
            decimal currentReturn = 0;

            // Solar generation (only during daylight hours)
            decimal solarGeneration = 0;
            if (hasSolar && hour >= 8 && hour <= 18)
            {
                var solarFactor = 1.0 - Math.Abs(hour - 13) / 5.0;
                solarGeneration = (decimal)(3.0 * solarFactor * _random.NextDouble());
                currentReturn = solarGeneration > baseUsage ? solarGeneration - baseUsage : 0;
            }

            var netUsage = baseUsage - solarGeneration;

            // Determine if importing or exporting
            if (netUsage > 0)
            {
                if (hour >= 21 || hour < 7) // Low tariff (night)
                    deliveredT1 = netUsage;
                else
                    deliveredT2 = netUsage;
            }
            else
            {
                if (hour >= 21 || hour < 7)
                    returnedT1 = Math.Abs(netUsage);
                else
                    returnedT2 = Math.Abs(netUsage);
            }

            return (deliveredT1, deliveredT2, returnedT1, returnedT2, Math.Max(baseUsage, 0.1m), currentReturn);
        }

        private decimal GenerateGasConsumption(int hour, int month)
        {
            var monthFactor = month >= 10 || month <= 3 ? 2.0 : 0.5;
            var hourFactor = (hour >= 6 && hour <= 9) || (hour >= 17 && hour <= 22) ? 1.5 : 0.8;

            return (decimal)(0.1 * monthFactor * hourFactor * _random.NextDouble());
        }
    }
}
