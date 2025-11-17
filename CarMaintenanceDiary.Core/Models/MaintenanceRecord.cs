using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Core.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string? Workshop { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;       
        public ICollection<MaintenanceDocument> Documents { get; set; } = new List<MaintenanceDocument>();
    }
}
