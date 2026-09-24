using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Identity;
using CarMaintenanceDiary.Infrastructure.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IUserContext? _userContext;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserContext? userContext = null) 
            : base(options)
        {
            _userContext = userContext;
        }
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<FuelRecord> FuelEntries { get; set; } = null!;
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = null!;
        public DbSet<MaintenanceDocument> MaintenanceDocuments { get; set; } = null!;
        public DbSet<FuelPhoto> FuelPhotos { get; set; } = null!;
        public DbSet<VehiclePhoto> VehiclePhotos { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
               .HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(v => v.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
             .HasIndex(v => v.UserId);

            modelBuilder.Entity<MaintenanceDocument>()
                .HasOne(p => p.MaintenanceRecord)
                .WithMany(r => r.Documents)
                .HasForeignKey(p => p.MaintenanceRecordId);

            modelBuilder.Entity<MaintenanceDocument>()
            .Property(p => p.Data)
            .HasColumnType("varbinary(max)");

            modelBuilder.Entity<FuelPhoto>()
               .HasOne(p => p.FuelRecord)
               .WithMany(r => r.Photos)
               .HasForeignKey(p => p.FuelRecordId);

            modelBuilder.Entity<FuelPhoto>()
            .Property(p => p.Data)
            .HasColumnType("varbinary(max)");

            modelBuilder.Entity<VehiclePhoto>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.Photos)
                .HasForeignKey(p => p.VehicleId);

            modelBuilder.Entity<VehiclePhoto>()
                .Property(p => p.Data)
                .HasColumnType("varbinary(max)");

            //modelBuilder.Entity<Vehicle>().HasQueryFilter(v =>
            //_userContext == null ||
            //_userContext.IsInRole("Admin") ||
            //v.UserId == _userContext.GetCurrentUserId());
        }
    }
}

