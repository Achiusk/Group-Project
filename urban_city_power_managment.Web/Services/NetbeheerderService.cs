using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
  /// <summary>
    /// Service for Dutch electricity grid operators (Netbeheerders)
    /// Returns the correct operator based on location/postal code
    /// </summary>
  public interface INetbeheerderService
    {
     Task<Netbeheerder?> GetNetbeheerderByPostalCodeAsync(string postalCode);
Task<List<Netbeheerder>> GetAllNetbeheerdersAsync();
    }

    public class NetbeheerderService : INetbeheerderService
    {
        // Dutch grid operators with their service areas (postal code ranges)
        private static readonly List<Netbeheerder> _netbeheerders = new()
        {
         new Netbeheerder
   {
      Id = "liander",
   Name = "Liander",
    LogoUrl = "/images/netbeheerders/liander.png",
  PhoneNumber = "0800-9009",
     Website = "https://www.liander.nl",
              Email = "klantenservice@liander.nl",
    ServiceAreas = new List<string>
      {
// Noord-Holland, Gelderland, Flevoland, parts of Zuid-Holland, Friesland
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
   "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
       "38", "39", "40", "65", "66", "67", "68", "69", "70", "71",
  "72", "73", "74", "80", "81", "82", "83", "84", "85", "86",
   "87", "88", "89", "90", "91", "92", "93", "94", "95", "96",
"97", "98", "99"
       },
  SmartMeterRequestUrl = "https://www.liander.nl/slimme-meter"
 },
            new Netbeheerder
  {
       Id = "enexis",
   Name = "Enexis",
    LogoUrl = "/images/netbeheerders/enexis.png",
     PhoneNumber = "088-857 7777",
 Website = "https://www.enexis.nl",
              Email = "klantenservice@enexis.nl",
   ServiceAreas = new List<string>
          {
               // Noord-Brabant, Limburg, parts of Overijssel, Groningen, Drenthe
               "46", "47", "48", "49", "50", "51", "52", "53", "54", "55",
 "56", "57", "58", "59", "60", "61", "62", "63", "64",
              "74", "75", "76", "77", "78", "79"
     },
      SmartMeterRequestUrl = "https://www.enexis.nl/slimme-meter-aanvragen"
  },
        new Netbeheerder
       {
           Id = "stedin",
            Name = "Stedin",
    LogoUrl = "/images/netbeheerders/stedin.png",
  PhoneNumber = "0800-9009",
           Website = "https://www.stedin.net",
Email = "klantenservice@stedin.net",
  ServiceAreas = new List<string>
      {
          // Zuid-Holland, Utrecht, parts of Zeeland
      "29", "30", "31", "32", "33", "34", "35", "36", "37",
        "28", "41", "42", "43", "44", "45"
       },
   SmartMeterRequestUrl = "https://www.stedin.net/slimme-meter"
          },
new Netbeheerder
   {
        Id = "westlandinfra",
           Name = "Westland Infra",
       LogoUrl = "/images/netbeheerders/westlandinfra.png",
   PhoneNumber = "088-997 0097",
           Website = "https://www.westlandinfra.nl",
          Email = "klantenservice@westlandinfra.nl",
   ServiceAreas = new List<string>
       {
 // Westland region
                 "26"
      },
SmartMeterRequestUrl = "https://www.westlandinfra.nl/slimme-meter"
 },
 new Netbeheerder
            {
 Id = "coteq",
     Name = "Coteq Netbeheer",
             LogoUrl = "/images/netbeheerders/coteq.png",
  PhoneNumber = "088-856 8856",
            Website = "https://www.coteqnetbeheer.nl",
       Email = "info@coteqnetbeheer.nl",
               ServiceAreas = new List<string>
     {
           // Twente region
   "75", "76"
    },
          SmartMeterRequestUrl = "https://www.coteqnetbeheer.nl/slimme-meter"
   },
 new Netbeheerder
 {
                Id = "rendo",
  Name = "Rendo Netwerken",
      LogoUrl = "/images/netbeheerders/rendo.png",
     PhoneNumber = "0800-0736",
          Website = "https://www.rendo.nl",
           Email = "klantenservice@rendo.nl",
       ServiceAreas = new List<string>
        {
 // Parts of Drenthe and Overijssel
 "79", "77"
      },
         SmartMeterRequestUrl = "https://www.rendo.nl/slimme-meter"
    }
        };

        public Task<Netbeheerder?> GetNetbeheerderByPostalCodeAsync(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode) || postalCode.Length < 4)
    return Task.FromResult<Netbeheerder?>(null);

      // Get first 2 digits of postal code
    var prefix = postalCode.Substring(0, 2);

         // Find the netbeheerder that serves this area
        var netbeheerder = _netbeheerders.FirstOrDefault(n => 
 n.ServiceAreas.Contains(prefix));

            // Default to Enexis for Eindhoven area (56xx)
       if (netbeheerder == null && postalCode.StartsWith("56"))
          {
         netbeheerder = _netbeheerders.First(n => n.Id == "enexis");
   }

            // Fallback to Liander if no match found
            netbeheerder ??= _netbeheerders.First(n => n.Id == "liander");

        return Task.FromResult<Netbeheerder?>(netbeheerder);
      }

 public Task<List<Netbeheerder>> GetAllNetbeheerdersAsync()
        {
      return Task.FromResult(_netbeheerders);
        }
    }
}
