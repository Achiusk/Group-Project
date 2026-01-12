using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace urban_city_power_managment.Web.Models
{
    /// <summary>
    /// User account stored in database
    /// </summary>
    public class UserAccount
    {
        [Key]
 public int Id { get; set; }

        [Required]
     [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
   public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

      [Required]
        [StringLength(7)]
        public string PostalCode { get; set; } = string.Empty;

     [Required]
        [StringLength(10)]
        public string HouseNumber { get; set; } = string.Empty;

        [StringLength(10)]
        public string? HouseNumberAddition { get; set; }

 [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

    [Required]
        [EmailAddress]
    [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
  [StringLength(255)]
     public string PasswordHash { get; set; } = string.Empty;

        public bool HasSmartMeter { get; set; }

        [StringLength(50)]
        public string? NetbeheerderId { get; set; }

        [StringLength(100)]
        public string? NetbeheerderName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

        /// <summary>
        /// Full name display
      /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Full address display
        /// </summary>
    public string FullAddress => string.IsNullOrEmpty(HouseNumberAddition)
 ? $"{Street} {HouseNumber}, {PostalCode} {City}"
    : $"{Street} {HouseNumber}{HouseNumberAddition}, {PostalCode} {City}";

        /// <summary>
        /// Calculate age from date of birth
        /// </summary>
   public int Age
        {
       get
       {
    var today = DateTime.Today;
          var age = today.Year - DateOfBirth.Year;
     if (DateOfBirth.Date > today.AddYears(-age)) age--;
       return age;
  }
  }
    }

    /// <summary>
    /// Registration form model with validation
    /// </summary>
    public class RegistrationModel
    {
   [Required(ErrorMessage = "Voornaam is verplicht")]
      [StringLength(50, MinimumLength = 2, ErrorMessage = "Voornaam moet tussen 2 en 50 karakters zijn")]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s\-']+$", ErrorMessage = "Voornaam mag alleen letters bevatten (A-Z)")]
        public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Achternaam is verplicht")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Achternaam moet tussen 2 en 50 karakters zijn")]
     [RegularExpression(@"^[A-Za-zÀ-ÿ\s\-']+$", ErrorMessage = "Achternaam mag alleen letters bevatten (A-Z)")]
        public string LastName { get; set; } = string.Empty;

   [Required(ErrorMessage = "Geboortedatum is verplicht")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Postcode is verplicht")]
     [RegularExpression(@"^[1-9][0-9]{3}\s?[A-Za-z]{2}$", ErrorMessage = "Voer een geldige Nederlandse postcode in (bijv. 1234 AB)")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Huisnummer is verplicht")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Huisnummer mag alleen cijfers bevatten")]
        public string HouseNumber { get; set; } = string.Empty;

    [RegularExpression(@"^[A-Za-z0-9\-]*$", ErrorMessage = "Ongeldige toevoeging")]
    public string? HouseNumberAddition { get; set; }

    // Auto-filled from postal code lookup
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mailadres is verplicht")]
        [EmailAddress(ErrorMessage = "Voer een geldig e-mailadres in")]
        public string Email { get; set; } = string.Empty;

 [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [MinLength(8, ErrorMessage = "Wachtwoord moet minimaal 8 karakters zijn")]
      public string Password { get; set; } = string.Empty;

     [Required(ErrorMessage = "Bevestig je wachtwoord")]
        [Compare(nameof(Password), ErrorMessage = "Wachtwoorden komen niet overeen")]
        public string ConfirmPassword { get; set; } = string.Empty;

   [Required(ErrorMessage = "Geef aan of je een slimme meter hebt")]
        public bool? HasSmartMeter { get; set; }

   public bool AcceptTerms { get; set; }

        /// <summary>
        /// Validates that the user is at least 18 years old
        /// </summary>
    public bool IsAtLeast18YearsOld()
    {
 if (!DateOfBirth.HasValue) return false;
     var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
     if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
     return age >= 18;
    }

      /// <summary>
        /// Validates password strength
    /// </summary>
        public PasswordStrength GetPasswordStrength()
        {
        if (string.IsNullOrEmpty(Password)) return PasswordStrength.Invalid;

     int score = 0;

         // Length checks
            if (Password.Length >= 8) score++;
        if (Password.Length >= 12) score++;
            if (Password.Length >= 16) score++;

       // Character type checks
 if (Regex.IsMatch(Password, @"[a-z]")) score++; // lowercase
            if (Regex.IsMatch(Password, @"[A-Z]")) score++; // uppercase
      if (Regex.IsMatch(Password, @"[0-9]")) score++; // numbers
   if (Regex.IsMatch(Password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score++; // special chars

 return score switch
            {
                <= 2 => PasswordStrength.Weak,
      <= 4 => PasswordStrength.Fair,
            <= 5 => PasswordStrength.Good,
        _ => PasswordStrength.Strong
       };
        }

        /// <summary>
    /// Validates password meets minimum requirements
    /// </summary>
        public (bool IsValid, List<string> Errors) ValidatePassword()
{
   var errors = new List<string>();

       if (string.IsNullOrEmpty(Password))
            {
    errors.Add("Wachtwoord is verplicht");
     return (false, errors);
      }

            if (Password.Length < 8)
     errors.Add("Minimaal 8 karakters");

         if (!Regex.IsMatch(Password, @"[a-z]"))
    errors.Add("Minimaal één kleine letter (a-z)");

    if (!Regex.IsMatch(Password, @"[A-Z]"))
    errors.Add("Minimaal één hoofdletter (A-Z)");

          if (!Regex.IsMatch(Password, @"[0-9]"))
     errors.Add("Minimaal één cijfer (0-9)");

          if (!Regex.IsMatch(Password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
  errors.Add("Minimaal één speciaal teken (!@#$%^&*)");

            return (errors.Count == 0, errors);
 }
    }

    /// <summary>
    /// Login form model
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "E-mailadres is verplicht")]
    [EmailAddress(ErrorMessage = "Voer een geldig e-mailadres in")]
      public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
    public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Password strength levels
    /// </summary>
    public enum PasswordStrength
    {
        Invalid,
   Weak,
   Fair,
        Good,
        Strong
    }

    /// <summary>
    /// Dutch address from postal code lookup
    /// </summary>
    public class DutchAddress
    {
        public string PostalCode { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string? HouseNumberAddition { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
   public string Municipality { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
  }

    /// <summary>
    /// Dutch grid operator (Netbeheerder)
    /// </summary>
    public class Netbeheerder
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
     public List<string> ServiceAreas { get; set; } = new();
        public string SmartMeterRequestUrl { get; set; } = string.Empty;
    }
}
