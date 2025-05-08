using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class VehicleApiService : IVehicleService
    {
        private readonly HttpClient _http;

        public VehicleApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<VehicleDto>>("api/vehicles/getall") ?? new();
        }

        public async Task<int> AddAsync(VehicleDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/vehicles/add", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task UpdateAsync(VehicleDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/vehicles/update/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/vehicles/delete/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<VehicleDto>($"api/vehicles/get/{id}");
        }
    }
}
