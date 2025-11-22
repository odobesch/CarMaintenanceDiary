using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Core.Models
{
    public class VehiclePhoto
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = string.Empty;
    }
}
