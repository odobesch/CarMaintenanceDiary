using CarMaintenanceDiary.Shared.DTOs;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<List<VehicleDto>> GetAllAsync();
        Task<VehicleDto?> GetByIdAsync(int id);
        Task<int> AddAsync(VehicleDto vehicle);
        Task UpdateAsync(VehicleDto vehicle);
        Task DeleteAsync(int id);

        Task<List<FuelRecordDto>> GetFuelRecordsAsync(int vehicleId);
        Task AddFuelRecordAsync(int vehicleId, FuelRecordDto fuelRecord);
        Task UpdateFuelRecordAsync(FuelRecordDto record);
        Task DeleteFuelRecordAsync(int fuelRecordId);

        Task<List<MonthlyFuelSummaryDto>> GetMonthlyFuelSummariesAsync(int vehicleId);
        Task<List<YearlyFuelSummaryDto>> GetYearlyFuelSummariesAsync(int vehicleId);
    }
}
