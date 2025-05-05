using CarMaintenanceDiary.Shared.DTOs.Fuel;
using System.ComponentModel.DataAnnotations;

namespace CarMaintenanceDiary.Shared.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Make is required")]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter a valid 4-digit year")]
        public string Year { get; set; } = string.Empty;

        [Required(ErrorMessage = "License plate number is required")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "VIN is required")]
        [StringLength(17, MinimumLength = 11, ErrorMessage = "VIN must be between 11 and 17 characters")]
        public string VIN { get; set; } = string.Empty;
        public ICollection<MaintenanceRecordDto> MaintenanceRecords { get; set; } = new List<MaintenanceRecordDto>();
        public ICollection<FuelRecordDto> FuelRecords { get; set; } = new List<FuelRecordDto>();

        public double AverageFuelConsumption
        {
            get
            {
                if (FuelRecords.Count < 2)
                {
                    return 0;
                }

                var totalLiters = FuelRecords.Sum(fr => fr.Liters);
                var totalDistance = FuelRecords.Max(fr => fr.Odometer) - FuelRecords.Min(fr => fr.Odometer);

                return totalDistance > 0 ? (totalLiters / totalDistance) * 100 : 0;
            }
        }
    }
}
