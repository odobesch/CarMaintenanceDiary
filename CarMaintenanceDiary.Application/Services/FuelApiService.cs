using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class FuelApiService : IFuelService
    {
        private readonly HttpClient _http;
        private const long MaxUploadBytes = 10_000_000;
        public FuelApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<FuelRecordDto>> GetFuelRecordsAsync(int vehicleId)
        {
            return await _http.GetFromJsonAsync<List<FuelRecordDto>>($"api/fuel/{vehicleId}") ?? new();
        }

        public async Task<FuelRecordDto?> GetFuelRecordByIdAsync(int recordId)
        {
            return await _http.GetFromJsonAsync<FuelRecordDto>($"api/fuel/record/{recordId}");
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

        public async Task<int?> UploadPhotoAsync(int recordId, Stream fileStream, string fileName, string? contentType = null)
        {
            if (fileStream == null)
                return null;

            // Ensure stream is at beginning if seekable
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

            var res = await _http.PostAsync($"api/fuel/{recordId}/photos", content);

            if (!res.IsSuccessStatusCode)
                return null;

            // Expecting response like { id: 123 }
            try
            {
                var json = await res.Content.ReadFromJsonAsync<JsonElement?>();
                if (json.HasValue)
                {
                    if (json.Value.TryGetProperty("id", out var prop) || json.Value.TryGetProperty("Id", out prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var id))
                            return id;
                    }
                }
            }
            catch
            {
                // ignore parse errors
            }

            return null;
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            var res = await _http.DeleteAsync($"api/fuel/photos/{photoId}");
            res.EnsureSuccessStatusCode();
        }

        // Returns a URL that can be used as img src
        public string GetPhotoUrl(int photoId)
        {
            var baseAddr = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            return $"{baseAddr}/api/fuel/photos/{photoId}";
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
            return $"{baseAddr}/api/fuel/photos/{photoId}?{string.Join("&", qs)}";
        }
    }
}
