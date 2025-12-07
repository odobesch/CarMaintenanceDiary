using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Application.Services
{
    public class MaintenanceApiService : IMaintenanceService
    {
        private readonly HttpClient _http;
        private const long MaxUploadBytes = 20_000_000;
        private readonly IAuthTokenAccessor _tokenAccessor;

        public MaintenanceApiService(HttpClient http, IAuthTokenAccessor tokenAccessor)
        {
            _http = http;
            _tokenAccessor = tokenAccessor;
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await _tokenAccessor.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("[VehicleApiService] WARNING: Token is null or empty, NOT setting auth header!");
            }
        }

        public async Task<List<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(int vehicleId)
        {
            await SetAuthHeaderAsync();            
            var result = await _http.GetFromJsonAsync<List<MaintenanceRecordDto>>($"api/maintenance/vehicle/{vehicleId}/records");
            return result ?? new List<MaintenanceRecordDto>();
        }

        public async Task<MaintenanceRecordDto?> GetMaintenanceRecordByIdAsync(int id)
        {
            await SetAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<MaintenanceRecordDto?>($"api/maintenance/records/{id}");
            return result;
        }

        public async Task AddMaintenanceRecordAsync(int vehicleId, MaintenanceRecordDto dto)
        {
            await SetAuthHeaderAsync();
            var res = await _http.PostAsJsonAsync($"api/maintenance/vehicle/{vehicleId}/records", dto);
            res.EnsureSuccessStatusCode();
        }

        public async Task UpdateMaintenanceRecordAsync(MaintenanceRecordDto dto)
        {
            await SetAuthHeaderAsync();
            var res = await _http.PutAsJsonAsync($"api/maintenance/records/{dto.Id}", dto);
            res.EnsureSuccessStatusCode();
        }

        public async Task DeleteMaintenanceRecordAsync(int id)
        {
            await SetAuthHeaderAsync();
            var res = await _http.DeleteAsync($"api/maintenance/records/{id}");
            res.EnsureSuccessStatusCode();
        }

        public async Task<int?> UploadDocumentAsync(int maintenanceRecordId, Stream fileStream, string fileName, string? contentType = null)
        {            
            if (fileStream == null)
                return null;
            if (fileStream.CanSeek)
            {
                try
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                }
                catch { }
            }

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
            content.Add(streamContent, "file", fileName);

            await SetAuthHeaderAsync();            
            var res = await _http.PostAsync($"api/maintenance/records/{maintenanceRecordId}/documents", content);
            if (!res.IsSuccessStatusCode)
                return null;

            try
            {
                var json = await res.Content.ReadFromJsonAsync<JsonElement?>();
                if (json.HasValue && (json.Value.TryGetProperty("id", out var prop) || json.Value.TryGetProperty("Id", out prop)))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var id))
                        return id;
                }
            }
            catch { /* ignore parse errors */ }

            return null;
        }

        public async Task DeleteDocumentAsync(int documentId)
        {            
            await SetAuthHeaderAsync();
            var res = await _http.DeleteAsync($"api/maintenance/documents/{documentId}");
            res.EnsureSuccessStatusCode();
        }

        public async Task<List<int>> GetDocumentIdsAsync(int maintenanceRecordId)
        {
            await SetAuthHeaderAsync();
            var ids = await _http.GetFromJsonAsync<List<int>>($"api/maintenance/records/{maintenanceRecordId}/documents");
            return ids ?? new List<int>();
        }

        public async Task<List<MaintenanceDocumentDto>> GetDocumentsAsync(int maintenanceRecordId)
        {
            await SetAuthHeaderAsync();
            var list = await _http.GetFromJsonAsync<List<MaintenanceDocumentDto>>($"api/maintenance/records/{maintenanceRecordId}/documents");
            return list ?? new List<MaintenanceDocumentDto>();
        }

        public string GetDocumentUrl(int documentId, int? w = null, int? h = null, string mode = "crop", int dpr = 1, string? format = null, int? q = null)
        {
            var baseAddr = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            var qs = new List<string>();
            if (w.HasValue)
                qs.Add($"w={w.Value}");
            if (h.HasValue)
                qs.Add($"h={h.Value}");
            if (!string.IsNullOrWhiteSpace(mode))
                qs.Add($"mode={mode}");
            if (dpr > 1)
                qs.Add($"dpr={dpr}");
            if (!string.IsNullOrWhiteSpace(format))
                qs.Add($"format={format}");
            if (q.HasValue)
                qs.Add($"q={q.Value}");
            var query = qs.Any() ? "?" + string.Join("&", qs) : "";
            // GET api/maintenance/documents/{id}
            return $"{baseAddr}/api/maintenance/documents/{documentId}{query}";
        }

        public string GetDocumentDownloadUrl(int documentId)
        {
            var baseAddr = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            // GET api/maintenance/documents/{id}/download
            return $"{baseAddr}/api/maintenance/documents/{documentId}/download";
        }
    }
}
