namespace CarMaintenanceDiary.Shared.DTOs.Fuel
{
    public class MonthlyFuelSummaryDto
    {
         public int Year { get; set; }
        public int Month { get; set; }
        public double TotalLiters { get; set; }
        public decimal TotalCost { get; set; }
        public double AveragePricePerLiter => TotalLiters > 0 ? (double)TotalCost / TotalLiters : 0;
        public double AverageConsumptionPer100Km { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
}
