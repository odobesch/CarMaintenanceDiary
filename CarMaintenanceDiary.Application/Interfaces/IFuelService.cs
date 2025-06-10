using CarMaintenanceDiary.Shared.DTOs.Fuel;

namespace CarMaintenanceDiary.Application.Interfaces
{
    public interface IFuelService
    {
        Task<List<FuelRecordDto>> GetFuelRecordsAsync(int vehicleId);
        Task AddFuelRecordAsync(int vehicleId, FuelRecordDto record);
        Task UpdateFuelRecordAsync(FuelRecordDto record);
        Task DeleteFuelRecordAsync(int recordId);
        Task<List<MonthlyFuelSummaryDto>> GetMonthlyFuelSummariesAsync(int vehicleId);
        Task<List<YearlyFuelSummaryDto>> GetYearlyFuelSummariesAsync(int vehicleId);
        Task<FuelRecordDto?> GetFuelRecordByIdAsync(int recordId);
    }
}
