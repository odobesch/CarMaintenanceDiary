namespace CarMaintenanceDiary.Api.Models
{
    public class FuelRecordFormDto
    {
        public DateTime Date { get; set; }
        public double Odometer { get; set; }
        public double Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public bool FullTank { get; set;}
        public List<IFormFile>? Photos { get; set; }
    }
}
