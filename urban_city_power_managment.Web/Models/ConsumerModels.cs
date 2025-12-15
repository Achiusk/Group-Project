namespace urban_city_power_managment.Web.Models
{
    public enum VendorCategory
    {
        Isolatie,
        Zonnepanelen,
        Thuisbatterij,
        Warmtepomp,
    Laadpaal,
      EnergieAdvies,
  SmartHome,
        Glaswerk
    }

    public class Vendor
    {
      public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
      public VendorCategory Category { get; set; }
        public string CategoryIcon { get; set; } = "??";
        public string Phone { get; set; } = string.Empty;
     public string Email { get; set; } = string.Empty;
  public string Website { get; set; } = string.Empty;
     public string Address { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string City { get; set; } = "Eindhoven";
        public double Rating { get; set; }
     public int ReviewCount { get; set; }
        public bool IsVerified { get; set; }
  public bool OffersFreQuote { get; set; }
     public string LogoUrl { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
     public string PriceRange { get; set; } = string.Empty;
 }

    public class EnergyTip
    {
     public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
        public string DetailedAdvice { get; set; } = string.Empty;
        public TipCategory Category { get; set; }
 public int PotentialSavingsPercent { get; set; }
 public decimal EstimatedYearlySavingsEuro { get; set; }
   public string Difficulty { get; set; } = "Gemakkelijk";
        public string Investment { get; set; } = "Gratis";
        public int Priority { get; set; }
        public string IconEmoji { get; set; } = "??";
     public VendorCategory? RelatedVendorCategory { get; set; }
    }

  public enum TipCategory
    {
 Verwarming,
        Koeling,
     Verlichting,
   Apparaten,
Isolatie,
      ZonneEnergie,
   Water,
        Gedrag,
        SlimMeten
    }

    public class EnergyProfile
    {
      public string ConsumerId { get; set; } = string.Empty;
      public double AverageDailyConsumptionKwh { get; set; }
        public double AverageMonthlyConsumptionKwh { get; set; }
        public double PeakConsumptionKw { get; set; }
        public TimeSpan PeakUsageTime { get; set; }
        public double ComparedToAveragePercent { get; set; }
      public string ConsumptionLevel { get; set; } = "Gemiddeld";
        public bool HasSolarPanels { get; set; }
        public double SolarProductionKwh { get; set; }
      public double SelfConsumptionPercent { get; set; }
        public double AverageMonthlyGasM3 { get; set; }
    public double GasComparedToAveragePercent { get; set; }
        public int EfficiencyScore { get; set; }
      public int SustainabilityScore { get; set; }
public decimal EstimatedMonthlyElectricityCost { get; set; }
        public decimal EstimatedMonthlyGasCost { get; set; }
    public decimal PotentialMonthlySavings { get; set; }
    }
}
