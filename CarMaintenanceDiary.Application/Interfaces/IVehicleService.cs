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
        Task<bool> LicensePlateExistsAsync(string licensePlate);
        Task<int?> UploadPhotoAsync(int vehicleId, Stream fileStream, string fileName, string? contentType = null);
        Task DeletePhotoAsync(int photoId);
        string GetPhotoUrl(int photoId);
        Task<List<int>> GetPhotoIdsAsync(int vehicleId);
        string GetPhotoUrl(int photoId, int w, int h, string mode = "crop", int dpr = 1, string? format = null, int? q = null);
    }
}
