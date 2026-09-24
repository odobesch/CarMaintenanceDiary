using System.Collections.Generic;

namespace CarMaintenanceDiary.Core.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<FuelRecord> FuelEntries { get; set; } = new List<FuelRecord>();
        public ICollection<VehiclePhoto> Photos { get; set; } = new List<VehiclePhoto>();
    }
}
