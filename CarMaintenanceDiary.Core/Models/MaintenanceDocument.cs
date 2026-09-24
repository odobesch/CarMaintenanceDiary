using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Core.Models
{
    public class MaintenanceDocument
    {
        public int Id { get; set; }
        public int MaintenanceRecordId { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; } = null!;        
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;       
        public string FileName { get; set; } = string.Empty;
    }
}
