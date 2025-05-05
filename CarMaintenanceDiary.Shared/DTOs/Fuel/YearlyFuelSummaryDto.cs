namespace CarMaintenanceDiary.Shared.DTOs.Fuel
{
    public class YearlyFuelSummaryDto
    {
        public int Year { get; set; }
        public double TotalLiters { get; set; }
        public decimal TotalCost { get; set; }
        public double AveragePricePerLiter => TotalLiters > 0 ? (double)TotalCost / TotalLiters : 0;
        public double AverageConsumptionPer100Km { get; set; }
    }
}
