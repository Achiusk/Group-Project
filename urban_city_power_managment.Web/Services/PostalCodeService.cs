using System.Text.Json;
using System.Text.RegularExpressions;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Service for Dutch postal code lookup
    /// Returns address details based on postal code + house number
    /// </summary>
    public interface IPostalCodeService
    {
   Task<DutchAddress?> LookupAddressAsync(string postalCode, string houseNumber, string? addition = null);
        bool IsValidDutchPostalCode(string postalCode);
    string FormatPostalCode(string postalCode);
    }

    public class PostalCodeService : IPostalCodeService
  {
        private readonly ILogger<PostalCodeService> _logger;
     
        // Eindhoven area street data for demo (expand for production)
        private static readonly Dictionary<string, Dictionary<string, (string Street, string City)>> _addressDatabase = new()
     {
   // Eindhoven Centrum
   ["5611"] = new Dictionary<string, (string, string)>
       {
    ["AA"] = ("Vestdijk", "Eindhoven"),
    ["AB"] = ("Stratumseind", "Eindhoven"),
        ["AC"] = ("Markt", "Eindhoven"),
    ["AD"] = ("Rechtestraat", "Eindhoven"),
     ["AE"] = ("Hoogstraat", "Eindhoven"),
   ["BA"] = ("Keizersgracht", "Eindhoven"),
     ["BB"] = ("Dommelstraat", "Eindhoven"),
  ["BC"] = ("Ten Hagestraat", "Eindhoven"),
  ["CA"] = ("Kleine Berg", "Eindhoven"),
 ["CB"] = ("Grote Berg", "Eindhoven"),
   ["DA"] = ("Wilhelminaplein", "Eindhoven"),
    ["HA"] = ("Stationsplein", "Eindhoven"),
      ["HB"] = ("Stationsweg", "Eindhoven")
  },
            // Eindhoven Strijp
    ["5616"] = new Dictionary<string, (string, string)>
 {
  ["AA"] = ("Strijpsestraat", "Eindhoven"),
      ["AB"] = ("Glaslaan", "Eindhoven"),
   ["AC"] = ("Torenallee", "Eindhoven"),
 ["BA"] = ("Philitelaan", "Eindhoven"),
  ["BB"] = ("Klokgebouw", "Eindhoven"),
              ["CA"] = ("Beukenlaan", "Eindhoven"),
       ["NA"] = ("Strijp-S", "Eindhoven")
         },
     // Eindhoven Woensel
       ["5625"] = new Dictionary<string, (string, string)>
   {
     ["AA"] = ("Woenselse Markt", "Eindhoven"),
   ["AB"] = ("Kruisstraat", "Eindhoven"),
        ["AC"] = ("Leenderweg", "Eindhoven"),
    ["BA"] = ("Fellenoord", "Eindhoven"),
   ["BB"] = ("Insulindelaan", "Eindhoven"),
["CA"] = ("Boschdijk", "Eindhoven")
 },
      // Eindhoven Gestel
        ["5613"] = new Dictionary<string, (string, string)>
{
  ["AA"] = ("Gestelsestraat", "Eindhoven"),
  ["AB"] = ("Aalsterweg", "Eindhoven"),
        ["AC"] = ("Boutenslaan", "Eindhoven"),
   ["BA"] = ("Geldropseweg", "Eindhoven"),
         ["BB"] = ("Hertogstraat", "Eindhoven")
},
           // Eindhoven Tongelre
      ["5641"] = new Dictionary<string, (string, string)>
    {
   ["AA"] = ("Tongelresestraat", "Eindhoven"),
       ["AB"] = ("Limburglaan", "Eindhoven"),
     ["AC"] = ("Genneperweg", "Eindhoven"),
 ["BA"] = ("Frankrijkstraat", "Eindhoven")
        },
       // Eindhoven Meerhoven
       ["5658"] = new Dictionary<string, (string, string)>
 {
       ["AA"] = ("Meerhovendreef", "Eindhoven"),
   ["AB"] = ("Sliffertsestraat", "Eindhoven"),
  ["AC"] = ("Zandrijk", "Eindhoven"),
     ["BA"] = ("Humperdincklaan", "Eindhoven"),
    ["CA"] = ("Meerwater", "Eindhoven")
          },
  // Veldhoven
          ["5502"] = new Dictionary<string, (string, string)>
     {
          ["AA"] = ("Dorpstraat", "Veldhoven"),
       ["AB"] = ("Kempenbaan", "Veldhoven"),
        ["BA"] = ("De Run", "Veldhoven")
  },
  // Geldrop
     ["5664"] = new Dictionary<string, (string, string)>
       {
       ["AA"] = ("Heuvel", "Geldrop"),
  ["AB"] = ("Mierloseweg", "Geldrop"),
         ["BA"] = ("Bogardeind", "Geldrop")
      },
// Nuenen
  ["5671"] = new Dictionary<string, (string, string)>
    {
        ["AA"] = ("Berg", "Nuenen"),
 ["AB"] = ("Park", "Nuenen"),
  ["BA"] = ("Parkstraat", "Nuenen")
    },
 // Best
          ["5683"] = new Dictionary<string, (string, string)>
{
 ["AA"] = ("Hoofdstraat", "Best"),
           ["AB"] = ("Oirschotseweg", "Best"),
      ["BA"] = ("De Maas", "Best")
     }
        };

      public PostalCodeService(ILogger<PostalCodeService> logger)
        {
      _logger = logger;
  }

    public bool IsValidDutchPostalCode(string postalCode)
      {
    if (string.IsNullOrWhiteSpace(postalCode))
    return false;

  // Dutch postal code format: 1234 AB or 1234AB
         var cleanedCode = postalCode.Replace(" ", "").ToUpper();
return Regex.IsMatch(cleanedCode, @"^[1-9][0-9]{3}[A-Z]{2}$");
    }

 public string FormatPostalCode(string postalCode)
{
    if (string.IsNullOrWhiteSpace(postalCode))
         return string.Empty;

     var cleanedCode = postalCode.Replace(" ", "").ToUpper();
     if (cleanedCode.Length == 6)
         {
         return $"{cleanedCode.Substring(0, 4)} {cleanedCode.Substring(4, 2)}";
          }
 return postalCode.ToUpper();
        }

        public async Task<DutchAddress?> LookupAddressAsync(string postalCode, string houseNumber, string? addition = null)
  {
      if (!IsValidDutchPostalCode(postalCode))
            {
    _logger.LogWarning("Invalid postal code format: {PostalCode}", postalCode);
         return null;
    }

      var cleanedCode = postalCode.Replace(" ", "").ToUpper();
 var prefix = cleanedCode.Substring(0, 4);
 var suffix = cleanedCode.Substring(4, 2);

    // Try to find in our local database first
            if (_addressDatabase.TryGetValue(prefix, out var streets))
            {
      if (streets.TryGetValue(suffix, out var addressInfo))
         {
 return new DutchAddress
        {
      PostalCode = FormatPostalCode(postalCode),
           HouseNumber = houseNumber,
                 HouseNumberAddition = addition,
   Street = addressInfo.Street,
         City = addressInfo.City,
         Municipality = addressInfo.City,
     Province = GetProvinceByCity(addressInfo.City)
       };
       }
            }

 // Generate a reasonable fallback for Eindhoven area
     if (cleanedCode.StartsWith("56"))
  {
         return new DutchAddress
{
        PostalCode = FormatPostalCode(postalCode),
      HouseNumber = houseNumber,
     HouseNumberAddition = addition,
      Street = GetDefaultStreetForArea(prefix),
                 City = "Eindhoven",
  Municipality = "Eindhoven",
 Province = "Noord-Brabant"
    };
       }

 // For other areas, return a generic address
return await GenerateFallbackAddressAsync(cleanedCode, houseNumber, addition);
        }

        private string GetDefaultStreetForArea(string prefix)
        {
    return prefix switch
       {
   "5611" => "Vestdijk",
"5612" => "Stratumseind",
      "5613" => "Gestelsestraat",
      "5614" => "Bergen",
      "5615" => "Witte Dame",
           "5616" => "Strijpsestraat",
        "5617" => "Philipsweg",
     "5621" => "Woenselse Markt",
       "5622" => "Fellenoord",
        "5623" => "Winkelcentrum Woensel",
       "5624" => "Tongelresestraat",
 "5625" => "Kruisstraat",
"5626" => "Boschdijk",
        "5627" => "Hemelrijken",
      "5628" => "Achtse Barrier",
        "5629" => "Blixembosch",
          "5631" => "Vaartbroek",
     "5632" => "Meerhoven",
           "5641" => "Tongelre",
            "5642" => "Genneperparken",
          "5643" => "Karpen",
  "5644" => "Grasrijk",
      "5645" => "Blaarthem",
      "5651" => "Stadion",
      "5652" => "High Tech Campus",
  "5653" => "Esp",
          "5654" => "Flight Forum",
           "5655" => "Hurk",
  "5656" => "Kapelbeemd",
   "5657" => "Acht",
   "5658" => "Meerhovendreef",
     _ => "Hoofdstraat"
            };
   }

     private string GetProvinceByCity(string city)
        {
  return city.ToLower() switch
            {
           "eindhoven" or "veldhoven" or "geldrop" or "nuenen" or "best" or "helmond" => "Noord-Brabant",
                "amsterdam" or "haarlem" or "alkmaar" => "Noord-Holland",
    "rotterdam" or "den haag" or "delft" or "leiden" => "Zuid-Holland",
       "utrecht" or "amersfoort" => "Utrecht",
       "groningen" => "Groningen",
  "maastricht" => "Limburg",
  "arnhem" or "nijmegen" => "Gelderland",
   "enschede" or "zwolle" => "Overijssel",
        "leeuwarden" => "Friesland",
       _ => "Noord-Brabant"
     };
   }

        private Task<DutchAddress?> GenerateFallbackAddressAsync(string postalCode, string houseNumber, string? addition)
        {
     // Determine city based on postal code prefix
    var prefix = int.Parse(postalCode.Substring(0, 2));
         var (city, province) = prefix switch
            {
 >= 10 and <= 19 => ("Amsterdam", "Noord-Holland"),
     >= 20 and <= 29 => ("Den Haag", "Zuid-Holland"),
     >= 30 and <= 39 => ("Rotterdam", "Zuid-Holland"),
 >= 40 and <= 49 => ("Utrecht", "Utrecht"),
   >= 50 and <= 59 => ("Eindhoven", "Noord-Brabant"),
       >= 60 and <= 69 => ("Maastricht", "Limburg"),
  >= 70 and <= 79 => ("Arnhem", "Gelderland"),
     >= 80 and <= 89 => ("Zwolle", "Overijssel"),
  >= 90 and <= 99 => ("Groningen", "Groningen"),
        _ => ("Onbekend", "Onbekend")
            };

          return Task.FromResult<DutchAddress?>(new DutchAddress
    {
      PostalCode = FormatPostalCode(postalCode),
    HouseNumber = houseNumber,
           HouseNumberAddition = addition,
        Street = "Hoofdstraat",
        City = city,
      Municipality = city,
     Province = province
 });
        }
    }
}
