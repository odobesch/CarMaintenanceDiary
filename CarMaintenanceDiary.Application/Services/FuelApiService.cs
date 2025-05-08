using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class FuelApiService : IFuelService
    {
        private readonly HttpClient _http;

        public FuelApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<FuelRecordDto>> GetFuelRecordsAsync(int vehicleId)
        {
            return await _http.GetFromJsonAsync<List<FuelRecordDto>>($"api/fuel/{vehicleId}") ?? new();
        }

        public async Task AddFuelRecordAsync(int vehicleId, FuelRecordDto record)
        {
            var response = await _http.PostAsJsonAsync($"api/fuel/{vehicleId}", record);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateFuelRecordAsync(FuelRecordDto record)
        {
            var response = await _http.PutAsJsonAsync($"api/fuel/{record.Id}", record);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFuelRecordAsync(int recordId)
        {
            var response = await _http.DeleteAsync($"api/fuel/{recordId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<MonthlyFuelSummaryDto>> GetMonthlyFuelSummariesAsync(int vehicleId)
        {
            return await _http.GetFromJsonAsync<List<MonthlyFuelSummaryDto>>($"api/fuel/summary/monthly/{vehicleId}") ?? new();
        }

        public async Task<List<YearlyFuelSummaryDto>> GetYearlyFuelSummariesAsync(int vehicleId)
        {
            return await _http.GetFromJsonAsync<List<YearlyFuelSummaryDto>>($"api/fuel/summary/yearly/{vehicleId}") ?? new();
        }
    }
}
