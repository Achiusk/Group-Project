using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    public interface IEnergyTipsService
    {
        Task<List<EnergyTip>> GetAllTipsAsync();
        Task<List<EnergyTip>> GetPersonalizedTipsAsync(EnergyProfile profile);
        Task<List<EnergyTip>> GetTipsByCategoryAsync(TipCategory category);
        Task<EnergyProfile> AnalyzeConsumerProfileAsync(string consumerId, List<P1SensorData> readings);
    }

    public class EnergyTipsService : IEnergyTipsService
    {
        private readonly List<EnergyTip> _tips;

        public EnergyTipsService()
        {
            _tips = GenerateEnergyTips();
        }

        public Task<List<EnergyTip>> GetAllTipsAsync()
        {
            return Task.FromResult(_tips.OrderByDescending(t => t.Priority).ToList());
        }

        public Task<List<EnergyTip>> GetPersonalizedTipsAsync(EnergyProfile profile)
        {
            var tips = new List<EnergyTip>();

            // High consumption - focus on reduction
            if (profile.ComparedToAveragePercent > 10)
            {
                tips.AddRange(_tips.Where(t => t.Category == TipCategory.Apparaten || t.Category == TipCategory.Gedrag));
            }

            // No solar panels - suggest solar
            if (!profile.HasSolarPanels)
            {
                tips.AddRange(_tips.Where(t => t.Category == TipCategory.ZonneEnergie));
            }

            // High gas usage - focus on heating/insulation
            if (profile.GasComparedToAveragePercent > 15)
            {
                tips.AddRange(_tips.Where(t => t.Category == TipCategory.Verwarming || t.Category == TipCategory.Isolatie));
            }

            // Low efficiency score
            if (profile.EfficiencyScore < 50)
            {
                tips.AddRange(_tips.Where(t => t.Category == TipCategory.SlimMeten));
            }

            // Always include some general tips
            tips.AddRange(_tips.Where(t => t.Category == TipCategory.Gedrag).Take(3));

            return Task.FromResult(tips
                .DistinctBy(t => t.Id)
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.EstimatedYearlySavingsEuro)
                .Take(10)
                .ToList());
        }

        public Task<List<EnergyTip>> GetTipsByCategoryAsync(TipCategory category)
        {
            return Task.FromResult(_tips.Where(t => t.Category == category).ToList());
        }

        public Task<EnergyProfile> AnalyzeConsumerProfileAsync(string consumerId, List<P1SensorData> readings)
        {
            if (!readings.Any())
            {
                return Task.FromResult(GenerateMockProfile(consumerId));
            }

            var profile = new EnergyProfile
            {
                ConsumerId = consumerId,
                AverageDailyConsumptionKwh = (double)readings.Average(r => r.CurrentPowerConsumption) * 24,
                AverageMonthlyConsumptionKwh = (double)readings.Average(r => r.CurrentPowerConsumption) * 24 * 30,
                PeakConsumptionKw = (double)readings.Max(r => r.CurrentPowerConsumption),
                HasSolarPanels = readings.Any(r => r.CurrentPowerReturn > 0),
                SolarProductionKwh = (double)readings.Sum(r => r.CurrentPowerReturn),
                AverageMonthlyGasM3 = (double)readings.Average(r => r.GasConsumption)
            };

            // Calculate compared to average (Dutch average: ~250 kWh/month electricity, ~120 m³/month gas)
            profile.ComparedToAveragePercent = ((profile.AverageMonthlyConsumptionKwh - 250) / 250) * 100;
            profile.GasComparedToAveragePercent = ((profile.AverageMonthlyGasM3 - 120) / 120) * 100;

            // Determine consumption level
            profile.ConsumptionLevel = profile.ComparedToAveragePercent switch
            {
                < -20 => "Laag",
                < 10 => "Gemiddeld",
                < 30 => "Hoog",
                _ => "Zeer Hoog"
            };

            // Calculate scores
            profile.EfficiencyScore = CalculateEfficiencyScore(profile);
            profile.SustainabilityScore = CalculateSustainabilityScore(profile);

            // Estimate costs (Dutch energy prices 2024 - more realistic)
            // Electricity: ~€0.25-0.30/kWh, Gas: ~€1.20-1.40/m³
            profile.EstimatedMonthlyElectricityCost = (decimal)(profile.AverageMonthlyConsumptionKwh * 0.28);
            profile.EstimatedMonthlyGasCost = (decimal)(profile.AverageMonthlyGasM3 * 1.30);
            profile.PotentialMonthlySavings = (profile.EstimatedMonthlyElectricityCost + profile.EstimatedMonthlyGasCost) * 0.12m;

            return Task.FromResult(profile);
        }

        private int CalculateEfficiencyScore(EnergyProfile profile)
        {
            int score = 50; // Start at average

            // Adjust based on consumption
            if (profile.ComparedToAveragePercent < -20) score += 25;
            else if (profile.ComparedToAveragePercent < 0) score += 15;
            else if (profile.ComparedToAveragePercent > 30) score -= 25;
            else if (profile.ComparedToAveragePercent > 10) score -= 10;

            // Bonus for solar
            if (profile.HasSolarPanels) score += 15;

            // Gas efficiency
            if (profile.GasComparedToAveragePercent < -10) score += 10;
            else if (profile.GasComparedToAveragePercent > 20) score -= 10;

            return Math.Clamp(score, 0, 100);
        }

        private int CalculateSustainabilityScore(EnergyProfile profile)
        {
            int score = 40;

            if (profile.HasSolarPanels)
            {
                score += 30;
                if (profile.SelfConsumptionPercent > 50) score += 10;
            }

            if (profile.ComparedToAveragePercent < 0) score += 10;
            if (profile.GasComparedToAveragePercent < 0) score += 10;

            return Math.Clamp(score, 0, 100);
        }

        private EnergyProfile GenerateMockProfile(string consumerId)
        {
            var random = new Random();

            // Realistic Dutch household values
            var monthlyKwh = 200 + random.NextDouble() * 150; // 200-350 kWh/month
            var monthlyGas = 80 + random.NextDouble() * 80;   // 80-160 m³/month
            var hasSolar = random.Next(100) > 55; // ~45% has solar

            // Dutch energy prices 2024
            var electricityCost = (decimal)(monthlyKwh * 0.28);  // ~€0.28/kWh
            var gasCost = (decimal)(monthlyGas * 1.30);          // ~€1.30/m³

            return new EnergyProfile
            {
                ConsumerId = consumerId,
                AverageDailyConsumptionKwh = Math.Round(monthlyKwh / 30, 2),
                AverageMonthlyConsumptionKwh = Math.Round(monthlyKwh, 2),
                PeakConsumptionKw = Math.Round(random.NextDouble() * 2.5 + 0.8, 2), // 0.8-3.3 kW peak
                PeakUsageTime = TimeSpan.FromHours(17 + random.Next(4)), // 17:00-20:00
                ComparedToAveragePercent = Math.Round((monthlyKwh - 250) / 250 * 100, 1),
                ConsumptionLevel = monthlyKwh < 200 ? "Laag" : monthlyKwh < 300 ? "Gemiddeld" : "Hoog",
                HasSolarPanels = hasSolar,
                SolarProductionKwh = hasSolar ? Math.Round(random.NextDouble() * 200 + 150, 2) : 0, // 150-350 kWh if solar
                SelfConsumptionPercent = hasSolar ? Math.Round(random.NextDouble() * 30 + 35, 1) : 0, // 35-65%
                AverageMonthlyGasM3 = Math.Round(monthlyGas, 2),
                GasComparedToAveragePercent = Math.Round((monthlyGas - 120) / 120 * 100, 1),
                EfficiencyScore = random.Next(45, 80),
                SustainabilityScore = hasSolar ? random.Next(55, 85) : random.Next(35, 60),
                EstimatedMonthlyElectricityCost = Math.Round(electricityCost, 2),  // ~€56-98
                EstimatedMonthlyGasCost = Math.Round(gasCost, 2),     // ~€104-208
                PotentialMonthlySavings = Math.Round((electricityCost + gasCost) * 0.12m, 2) // ~12% savings potential
            };
        }

        private List<EnergyTip> GenerateEnergyTips()
        {
            return new List<EnergyTip>
            {
                // Verwarming
                new EnergyTip
                {
                    Title = "Verlaag de thermostaat met 1 graad",
                    Description = "Elke graad lager scheelt 6-7% op je gasrekening",
                    DetailedAdvice = "Zet de thermostaat overdag op 19-20°C en 's nachts op 15-16°C. Gebruik een slimme thermostaat voor automatische regeling.",
                    Category = TipCategory.Verwarming,
                    PotentialSavingsPercent = 7,
                    EstimatedYearlySavingsEuro = 120,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 5,
                    IconEmoji = "???"
                },
                new EnergyTip
                {
                    Title = "Installeer een slimme thermostaat",
                    Description = "Bespaar tot 15% op verwarming met automatische regeling",
                    DetailedAdvice = "Een slimme thermostaat leert je gedrag en optimaliseert automatisch. Populaire opties: Tado, Nest, Honeywell.",
                    Category = TipCategory.Verwarming,
                    PotentialSavingsPercent = 15,
                    EstimatedYearlySavingsEuro = 180,
                    Difficulty = "Gemakkelijk",
                    Investment = "Laag (< €100)",
                    Priority = 4,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.SmartHome
                },
                new EnergyTip
                {
                    Title = "Ontlucht je radiatoren",
                    Description = "Lucht in radiatoren vermindert de warmteoverdracht",
                    DetailedAdvice = "Ontlucht radiatoren aan het begin van het stookseizoen. Hoor je borrelen? Dan zit er lucht in.",
                    Category = TipCategory.Verwarming,
                    PotentialSavingsPercent = 5,
                    EstimatedYearlySavingsEuro = 60,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 3,
                    IconEmoji = "??"
                },

                // Isolatie
                new EnergyTip
                {
                    Title = "Isoleer je spouwmuren",
                    Description = "Tot 35% besparing op verwarmingskosten",
                    DetailedAdvice = "Spouwmuurisolatie is een van de meest effectieve maatregelen. Subsidie beschikbaar via ISDE.",
                    Category = TipCategory.Isolatie,
                    PotentialSavingsPercent = 35,
                    EstimatedYearlySavingsEuro = 450,
                    Difficulty = "Complex",
                    Investment = "Hoog (> €1000)",
                    Priority = 5,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.Isolatie
                },
                new EnergyTip
                {
                    Title = "Plaats tochtstrips",
                    Description = "Voorkom warmteverlies door kieren en naden",
                    DetailedAdvice = "Breng tochtstrips aan bij ramen en deuren. Kost weinig maar bespaart veel.",
                    Category = TipCategory.Isolatie,
                    PotentialSavingsPercent = 8,
                    EstimatedYearlySavingsEuro = 95,
                    Difficulty = "Gemakkelijk",
                    Investment = "Laag (< €100)",
                    Priority = 4,
                    IconEmoji = "??"
                },
                new EnergyTip
                {
                    Title = "HR++ of triple glas",
                    Description = "Modern glas isoleert 3x beter dan enkel glas",
                    DetailedAdvice = "Vervang enkel glas door HR++ of triple glas. Combineert goed met kozijnvervanging.",
                    Category = TipCategory.Isolatie,
                    PotentialSavingsPercent = 20,
                    EstimatedYearlySavingsEuro = 280,
                    Difficulty = "Complex",
                    Investment = "Hoog (> €1000)",
                    Priority = 4,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.Glaswerk
                },

                // Zonne-energie
                new EnergyTip
                {
                    Title = "Installeer zonnepanelen",
                    Description = "Wek je eigen stroom op en bespaar honderden euro's per jaar",
                    DetailedAdvice = "Gemiddeld 8-12 panelen is voldoende voor een huishouden. Terugverdientijd: 5-7 jaar.",
                    Category = TipCategory.ZonneEnergie,
                    PotentialSavingsPercent = 50,
                    EstimatedYearlySavingsEuro = 600,
                    Difficulty = "Complex",
                    Investment = "Hoog (> €1000)",
                    Priority = 5,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.Zonnepanelen
                },
                new EnergyTip
                {
                    Title = "Overweeg een thuisbatterij",
                    Description = "Sla zonne-energie op voor 's avonds",
                    DetailedAdvice = "Vooral interessant bij afbouw salderingsregeling. Verhoogt eigen verbruik van zonnestroom.",
                    Category = TipCategory.ZonneEnergie,
                    PotentialSavingsPercent = 20,
                    EstimatedYearlySavingsEuro = 240,
                    Difficulty = "Complex",
                    Investment = "Hoog (> €1000)",
                    Priority = 3,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.Thuisbatterij
                },

                // Apparaten
                new EnergyTip
                {
                    Title = "Vervang oude apparaten door A+++ modellen",
                    Description = "Nieuwe apparaten gebruiken tot 50% minder stroom",
                    DetailedAdvice = "Focus eerst op koelkast, wasmachine en droger - deze gebruiken het meeste stroom.",
                    Category = TipCategory.Apparaten,
                    PotentialSavingsPercent = 15,
                    EstimatedYearlySavingsEuro = 120,
                    Difficulty = "Gemiddeld",
                    Investment = "Hoog (> €1000)",
                    Priority = 3,
                    IconEmoji = "??"
                },
                new EnergyTip
                {
                    Title = "Elimineer sluipverbruik",
                    Description = "Apparaten op standby kosten €30-60 per jaar",
                    DetailedAdvice = "Gebruik schakelbare stekkerdozen. Zet TV, computer en opladers helemaal uit.",
                    Category = TipCategory.Apparaten,
                    PotentialSavingsPercent = 5,
                    EstimatedYearlySavingsEuro = 45,
                    Difficulty = "Gemakkelijk",
                    Investment = "Laag (< €100)",
                    Priority = 4,
                    IconEmoji = "??"
                },
                new EnergyTip
                {
                    Title = "Was op 30°C",
                    Description = "Verlaag wastemperatuur voor 40% energiebesparing",
                    DetailedAdvice = "Moderne wasmiddelen werken uitstekend op 30°C. Reserveer 60°C voor beddengoed.",
                    Category = TipCategory.Apparaten,
                    PotentialSavingsPercent = 4,
                    EstimatedYearlySavingsEuro = 30,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 3,
                    IconEmoji = "??"
                },
                // Verlichting
                new EnergyTip
                {
                    Title = "Vervang alle lampen door LED",
                    Description = "LED gebruikt 80% minder stroom dan gloeilampen",
                    DetailedAdvice = "LED gaat 15-25 jaar mee. Begin met de lampen die het vaakst branden.",
                    Category = TipCategory.Verlichting,
                    PotentialSavingsPercent = 8,
                    EstimatedYearlySavingsEuro = 65,
                    Difficulty = "Gemakkelijk",
                    Investment = "Laag (< €100)",
                    Priority = 4,
                    IconEmoji = "??"
                },
                // Water
                new EnergyTip
                {
                    Title = "Douche korter (max 5 minuten)",
                    Description = "Korter douchen bespaart gas én water",
                    DetailedAdvice = "Een waterbesparende douchekop helpt ook. Kost €20-40, bespaart €60/jaar.",
                    Category = TipCategory.Water,
                    PotentialSavingsPercent = 10,
                    EstimatedYearlySavingsEuro = 110,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 4,
                    IconEmoji = "??"
                },
                // Gedrag
                new EnergyTip
                {
                    Title = "Monitor je verbruik dagelijks",
                    Description = "Bewustwording alleen al bespaart 5-10%",
                    DetailedAdvice = "Check dagelijks je P1 meter data. Zoek naar onverklaarbaar hoog verbruik.",
                    Category = TipCategory.Gedrag,
                    PotentialSavingsPercent = 8,
                    EstimatedYearlySavingsEuro = 85,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 5,
                    IconEmoji = "??"
                },
                new EnergyTip
                {
                    Title = "Gebruik apparaten op dal-uren",
                    Description = "Stroom is goedkoper buiten piekuren (bijv. 's nachts)",
                    DetailedAdvice = "Plan wasmachine, droger en vaatwasser na 23:00 of voor 07:00 als je dynamisch tarief hebt.",
                    Category = TipCategory.Gedrag,
                    PotentialSavingsPercent = 10,
                    EstimatedYearlySavingsEuro = 75,
                    Difficulty = "Gemakkelijk",
                    Investment = "Gratis",
                    Priority = 3,
                    IconEmoji = "?"
                },
                new EnergyTip
                {
                    Title = "Installeer energie monitoring",
                    Description = "Zie precies waar je energie naartoe gaat",
                    DetailedAdvice = "Een energiemonitor op groepsniveau toont welke apparaten veel verbruiken.",
                    Category = TipCategory.SlimMeten,
                    PotentialSavingsPercent = 12,
                    EstimatedYearlySavingsEuro = 120,
                    Difficulty = "Gemiddeld",
                    Investment = "Gemiddeld (€100-1000)",
                    Priority = 4,
                    IconEmoji = "??",
                    RelatedVendorCategory = VendorCategory.SmartHome
                }
            };
        }
    }
}
