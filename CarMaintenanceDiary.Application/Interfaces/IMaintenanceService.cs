using CarMaintenanceDiary.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<List<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(int vehicleId);
        Task<MaintenanceRecordDto?> GetMaintenanceRecordByIdAsync(int id);
        Task AddMaintenanceRecordAsync(int vehicleId, MaintenanceRecordDto dto);
        Task UpdateMaintenanceRecordAsync(MaintenanceRecordDto dto);
        Task DeleteMaintenanceRecordAsync(int id);        
        Task<int?> UploadDocumentAsync(int maintenanceRecordId, Stream fileStream, string fileName, string? contentType = null);
        Task DeleteDocumentAsync(int documentId);
        Task<List<int>> GetDocumentIdsAsync(int maintenanceRecordId);
        Task<List<MaintenanceDocumentDto>> GetDocumentsAsync(int maintenanceRecordId);
        string GetDocumentUrl(int documentId, int? w = null, int? h = null, string mode = "crop", int dpr = 1, string? format = null, int? q = null);
        string GetDocumentDownloadUrl(int documentId);
    }
}
