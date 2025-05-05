using System.ComponentModel.DataAnnotations;

namespace CarMaintenanceDiary.Shared.DTOs.Fuel
{
    public class FuelRecordDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "ODO value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Odometer must be greater than 0")]
        public double Odometer { get; set; }

        [Required(ErrorMessage = "Liter value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Liters must be greater than 0")]
        public double Liters { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerLiter { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public bool FullTank { get; set; }
    }
}
