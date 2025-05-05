using CarMaintenanceDiary.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<FuelRecord> FuelEntries { get; set; } = null!;
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = null!;
        public DbSet<MaintenancePhoto> MaintenancePhotos { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any additional configuration here
            modelBuilder.Entity<MaintenancePhoto>()
           .HasOne(p => p.MaintenanceRecord)
           .WithMany(r => r.Photos)
           .HasForeignKey(p => p.MaintenanceRecordId);
        }
    }
}

