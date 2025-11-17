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
        Task<int?> UploadPhotoAsync(int recordId, Stream fileStream, string fileName, string? contentType = null);
        Task DeletePhotoAsync(int photoId);
        string GetPhotoUrl(int photoId);
        string GetPhotoUrl(int photoId, int w, int h, string mode = "crop", int dpr = 1, string? format = null, int? q = null);
    }
}
