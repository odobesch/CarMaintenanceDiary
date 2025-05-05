using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Shared.DTOs
{
    public class MaintenanceRecordDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string? Workshop { get; set; }
        public string MaintenanceType { get; set; } = string.Empty; // You may later use an enum
        public List<string> PhotoPaths { get; set; } = new(); // File paths for uploaded photos
    }
}
