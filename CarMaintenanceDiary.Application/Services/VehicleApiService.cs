using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class VehicleApiService : IVehicleService
    {
        private readonly HttpClient _http;
        private const long MaxUploadBytes = 10_000_000;
        public VehicleApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<VehicleDto>>("api/vehicles/getall") ?? new();
            return result;
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate)
        {
            var result = await _http.GetFromJsonAsync<bool>($"api/vehicles/exists/{licensePlate}");
            return result;
        }

        public async Task<int> AddAsync(VehicleDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/vehicles", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task UpdateAsync(VehicleDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/vehicles/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/vehicles/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            var result = await _http.GetFromJsonAsync<VehicleDto>($"api/vehicles/{id}");
            return result;
        }

        public async Task<int?> UploadPhotoAsync(int vehicleId, Stream fileStream, string fileName, string? contentType = null)
        {
            if (fileStream == null)
                return null;
            if (fileStream.CanSeek)
                try
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                }
                catch { }

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
            content.Add(streamContent, "file", fileName);

            var res = await _http.PostAsync($"api/vehicles/{vehicleId}/photos", content);
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
            catch { }
            return null;
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            var res = await _http.DeleteAsync($"api/vehicles/photos/{photoId}");
            res.EnsureSuccessStatusCode();
        }

        public string GetPhotoUrl(int photoId)
        {
            var baseAddr = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            return $"{baseAddr}/api/vehicles/photos/{photoId}";
        }

        public string GetPhotoUrl(int photoId, int w, int h, string mode = "crop", int dpr = 1, string? format = null, int? q = null)
        {
            var baseAddr = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            var qs = new List<string> { $"w={w}", $"h={h}" };
            if (!string.IsNullOrWhiteSpace(mode))
                qs.Add($"mode={mode}");
            if (dpr > 1)
                qs.Add($"dpr={dpr}");
            if (!string.IsNullOrWhiteSpace(format))
                qs.Add($"format={format}");
            if (q is not null)
                qs.Add($"q={q}");
            return $"{baseAddr}/api/vehicles/photos/{photoId}?{string.Join("&", qs)}";
        }

        // Added: call the API endpoint that returns the photo id list
        public async Task<List<int>> GetPhotoIdsAsync(int vehicleId)
        {
            var ids = await _http.GetFromJsonAsync<List<int>>($"api/vehicles/{vehicleId}/photos");
            return ids ?? new List<int>();
        }               
    }
}
