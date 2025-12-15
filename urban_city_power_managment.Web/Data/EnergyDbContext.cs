using Microsoft.EntityFrameworkCore;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Eindhoven Energy Management
    /// Connects to Azure SQL Database
    /// </summary>
    public class EnergyDbContext : DbContext
    {
     public EnergyDbContext(DbContextOptions<EnergyDbContext> options) : base(options)
  {
        }

        // P1 Sensor Data
        public DbSet<P1SensorData> P1SensorData { get; set; }
        public DbSet<Consumer> Consumers { get; set; }

     // Gas Infrastructure
        public DbSet<GasUsage> GasUsage { get; set; }
        public DbSet<GasLeakAlert> GasLeakAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
base.OnModelCreating(modelBuilder);

         // P1SensorData configuration
modelBuilder.Entity<P1SensorData>(entity =>
            {
       entity.HasIndex(e => e.ConsumerId);
 entity.HasIndex(e => e.Timestamp);
    entity.HasIndex(e => e.Wijk);
    entity.HasIndex(e => new { e.ConsumerId, e.Timestamp });

     // Relationship with Consumer
     entity.HasOne(e => e.Consumer)
          .WithMany(c => c.SensorReadings)
        .HasForeignKey(e => e.ConsumerId)
          .OnDelete(DeleteBehavior.Cascade);
            });

     // Consumer configuration
   modelBuilder.Entity<Consumer>(entity =>
   {
entity.HasIndex(e => e.Wijk);
        entity.HasIndex(e => e.Postcode);
       });

      // GasUsage configuration
    modelBuilder.Entity<GasUsage>(entity =>
     {
         entity.HasIndex(e => e.Location);
    entity.HasIndex(e => e.Timestamp);
      });

     // GasLeakAlert configuration
    modelBuilder.Entity<GasLeakAlert>(entity =>
      {
           entity.HasIndex(e => e.Location);
 entity.HasIndex(e => e.DetectedAt);
 entity.HasIndex(e => e.IsResolved);
            entity.HasIndex(e => e.Severity);
          });

      // Seed sample data for Eindhoven wijken
       SeedConsumers(modelBuilder);
  }

      private void SeedConsumers(ModelBuilder modelBuilder)
        {
    var wijken = new[] { "Strijp", "Woensel-Noord", "Woensel-Zuid", "Gestel", "Stratum", "Tongelre", "Centrum", "Meerhoven" };
            var streets = new[] { "Vestdijk", "Stratumseind", "Hoogstraat", "Markt", "Willemstraat", "Strijpsestraat", "Woenselsestraat", "Fellenoord" };
        var random = new Random(42); // Fixed seed for reproducible data

   var consumers = new List<Consumer>();
  int id = 1;

            foreach (var wijk in wijken)
    {
    for (int i = 0; i < 10; i++)
    {
     consumers.Add(new Consumer
            {
    ConsumerId = $"CONS-{id:D4}",
      DisplayName = $"Huishouden {wijk} #{i + 1}",
       Address = $"{streets[random.Next(streets.Length)]} {random.Next(1, 200)}, Eindhoven",
      Wijk = wijk,
                  Postcode = $"56{random.Next(10, 99)} {(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}",
   HasSolarPanels = random.Next(100) > 60, // 40% have solar
     CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365)),
     IsActive = true
            });
       id++;
                }
 }

  modelBuilder.Entity<Consumer>().HasData(consumers);
   }
    }
}
