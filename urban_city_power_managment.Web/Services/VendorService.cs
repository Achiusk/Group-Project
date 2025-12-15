using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    public interface IVendorService
    {
        Task<List<Vendor>> GetAllVendorsAsync();
    Task<List<Vendor>> GetVendorsByCategoryAsync(VendorCategory category);
 Task<Vendor?> GetVendorByIdAsync(string id);
        Task<List<Vendor>> SearchVendorsAsync(string searchTerm);
    Task<List<Vendor>> GetFeaturedVendorsAsync();
        string GetCategoryIcon(VendorCategory category);
    }

    public class VendorService : IVendorService
    {
        private readonly List<Vendor> _vendors;

        public VendorService()
        {
            _vendors = GenerateSampleVendors();
        }

        public Task<List<Vendor>> GetAllVendorsAsync()
        {
            return Task.FromResult(_vendors);
        }

        public Task<List<Vendor>> GetVendorsByCategoryAsync(VendorCategory category)
        {
            var result = _vendors.Where(v => v.Category == category).ToList();
  return Task.FromResult(result);
        }

        public Task<Vendor?> GetVendorByIdAsync(string id)
        {
    var vendor = _vendors.FirstOrDefault(v => v.Id == id);
    return Task.FromResult(vendor);
     }

        public Task<List<Vendor>> SearchVendorsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
      var result = _vendors.Where(v =>
       v.Name.ToLower().Contains(term) ||
             v.Description.ToLower().Contains(term) ||
        v.Services.Any(s => s.ToLower().Contains(term))
        ).ToList();
        return Task.FromResult(result);
        }

        public Task<List<Vendor>> GetFeaturedVendorsAsync()
        {
     var result = _vendors
     .Where(v => v.IsVerified && v.Rating >= 4.0)
       .OrderByDescending(v => v.Rating)
            .Take(6)
        .ToList();
            return Task.FromResult(result);
        }

        public string GetCategoryIcon(VendorCategory category)
        {
   return category switch
         {
       VendorCategory.Isolatie => "\U0001F9F1",
     VendorCategory.Zonnepanelen => "\u2600\uFE0F",
      VendorCategory.Thuisbatterij => "\U0001F50B",
           VendorCategory.Warmtepomp => "\U0001F321\uFE0F",
                VendorCategory.Laadpaal => "\U0001F50C",
        VendorCategory.EnergieAdvies => "\U0001F4CB",
           VendorCategory.SmartHome => "\U0001F4F1",
       VendorCategory.Glaswerk => "\U0001FA9F",
       _ => "\U0001F3E2"
            };
        }

        private List<Vendor> GenerateSampleVendors()
        {
  return new List<Vendor>
        {
                CreateVendor(
              "Eindhoven Isolatie Experts",
  "Specialist in spouwmuur-, dak- en vloerisolatie voor woningen in de regio Eindhoven",
    VendorCategory.Isolatie,
     "040-123 4567", "info@eindhovenisolatie.nl", "https://eindhovenisolatie.nl",
           "Industrieweg 15", "5652 AA", 4.7, 234, true, true,
 new List<string> { "Spouwmuurisolatie", "Dakisolatie", "Vloerisolatie", "HR++ glas" }, "\u20AC\u20AC"),
                CreateVendor(
  "IsoTherm Brabant",
          "Duurzame isolatieoplossingen met focus op ecologische materialen",
            VendorCategory.Isolatie,
     "040-234 5678", "contact@isothermbrabant.nl", "https://isothermbrabant.nl",
       "Kanaaldijk 45", "5617 BC", 4.5, 156, true, true,
             new List<string> { "Ecologische isolatie", "Cellulose", "Houtvezel", "Energielabel advies" }, "\u20AC\u20AC\u20AC"),
  CreateVendor(
                    "SolarCity Eindhoven",
   "Complete zonnepaneel installaties met 25 jaar garantie",
    VendorCategory.Zonnepanelen,
        "040-345 6789", "info@solarcity-eindhoven.nl", "https://solarcity-eindhoven.nl",
       "Strijpsestraat 100", "5616 GK", 4.8, 412, true, true,
    new List<string> { "Zonnepanelen", "Omvormers", "Monitoring systeem", "Onderhoud" }, "\u20AC\u20AC"),
                CreateVendor(
      "Zonkracht Brabant",
          "Lokale specialist in zonne-energie voor particulieren en VvE's",
    VendorCategory.Zonnepanelen,
          "040-456 7890", "info@zonkrachtbrabant.nl", "https://zonkrachtbrabant.nl",
         "Markt 25", "5611 EC", 4.6, 287, true, true,
     new List<string> { "Zonnepanelen", "VvE projecten", "Lease opties", "Subsidie advies" }, "\u20AC\u20AC"),
      CreateVendor(
     "PowerHome Solutions",
   "Thuisbatterijen en energieopslag systemen voor optimaal eigen verbruik",
VendorCategory.Thuisbatterij,
           "040-567 8901", "info@powerhome.nl", "https://powerhome.nl",
         "Techniekweg 8", "5657 EG", 4.4, 98, true, true,
         new List<string> { "Tesla Powerwall", "BYD batterijen", "Hybride systemen", "Smart energy management" }, "\u20AC\u20AC\u20AC"),
       CreateVendor(
       "Warmte Totaal Eindhoven",
          "Erkend installateur van warmtepompen en hybride systemen",
      VendorCategory.Warmtepomp,
         "040-678 9012", "info@warmtetotaal.nl", "https://warmtetotaal-eindhoven.nl",
       "Industrielaan 50", "5651 GH", 4.6, 189, true, true,
       new List<string> { "Lucht-water warmtepomp", "Hybride warmtepomp", "Vloerverwarming", "ISDE subsidie" }, "\u20AC\u20AC\u20AC"),
          CreateVendor(
        "EcoWarmte Brabant",
        "Specialist in duurzame verwarmingsoplossingen sinds 2010",
          VendorCategory.Warmtepomp,
     "040-789 0123", "contact@ecowarmte.nl", "https://ecowarmtebrabant.nl",
      "Groenstraat 12", "5623 AB", 4.7, 156, true, true,
new List<string> { "Warmtepompen", "Bodem-warmte", "Zonneboiler", "Energieadvies" }, "\u20AC\u20AC\u20AC"),
     CreateVendor(
 "ChargePoint Eindhoven",
     "Laadpalen voor thuis en bedrijf met slimme laadoplossingen",
  VendorCategory.Laadpaal,
        "040-890 1234", "info@chargepoint-ehv.nl", "https://chargepoint-eindhoven.nl",
      "Autoweg 30", "5627 JK", 4.5, 134, true, true,
   new List<string> { "Thuislaadpaal", "Slim laden", "Zonnepaneel integratie", "Zakelijk laden" }, "\u20AC\u20AC"),
              CreateVendor(
   "Energie Advies Centrum Eindhoven",
      "Onafhankelijk advies voor energiebesparing en verduurzaming",
                    VendorCategory.EnergieAdvies,
               "040-901 2345", "advies@eac-eindhoven.nl", "https://energieadvies-eindhoven.nl",
       "Stadhuisplein 1", "5611 EM", 4.9, 567, true, true,
     new List<string> { "Energiescan", "Maatwerkadvies", "Subsidie hulp", "Energielabel" }, "\u20AC"),
         CreateVendor(
   "SmartLiving Eindhoven",
        "Slimme thermostaten en home automation voor energiebesparing",
         VendorCategory.SmartHome,
            "040-012 3456", "info@smartliving-ehv.nl", "https://smartliving-eindhoven.nl",
        "Technopark 5", "5657 DW", 4.3, 89, true, false,
   new List<string> { "Slimme thermostaat", "Home Assistant", "Energiemonitoring", "Automatisering" }, "\u20AC\u20AC"),
          CreateVendor(
     "Glashuis Brabant",
         "HR++ en triple glas voor betere isolatie en comfort",
   VendorCategory.Glaswerk,
         "040-111 2222", "info@glashuisbrabant.nl", "https://glashuisbrabant.nl",
          "Glaslaan 18", "5616 LW", 4.4, 178, true, true,
          new List<string> { "HR++ beglazing", "Triple glas", "Kozijnen", "Deuren" }, "\u20AC\u20AC")
     };
  }

        private Vendor CreateVendor(string name, string description, VendorCategory category,
   string phone, string email, string website, string address, string postcode,
            double rating, int reviewCount, bool isVerified, bool offersQuote,
      List<string> services, string priceRange)
        {
        return new Vendor
  {
       Name = name,
      Description = description,
      Category = category,
        CategoryIcon = GetCategoryIcon(category),
          Phone = phone,
                Email = email,
  Website = website,
   Address = address,
      Postcode = postcode,
    Rating = rating,
           ReviewCount = reviewCount,
         IsVerified = isVerified,
       OffersFreQuote = offersQuote,
       Services = services,
        PriceRange = priceRange
       };
      }
    }
}
