namespace CarMaintenanceDiary.Core.Models
{
    public class FuelRecord
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public DateTime Date { get; set; }
        public double Odometer { get; set; }
        public double Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public bool FullTank { get; set; }
        public ICollection<FuelPhoto> Photos { get; set; } = new List<FuelPhoto>();
    }
}
