using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Core.Models
{
    public class MaintenancePhoto
    {
        public int Id { get; set; }

        public int MaintenanceRecordId { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; } = null!;

        public string FilePath { get; set; } = string.Empty;
    }
}
