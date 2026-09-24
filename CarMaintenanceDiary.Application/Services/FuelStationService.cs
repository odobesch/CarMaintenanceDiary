using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class FuelStationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<FuelStationService> _logger;

        public FuelStationService(IHttpClientFactory factory, ILogger<FuelStationService> logger)
        {
            _http = factory.CreateClient("FuelStation");
            _logger = logger;
        }

        public async Task<List<FuelStationDto>> GetNearbyFuelStationsAsync(double lat, double lon, int radiusMeters = 5000)
        {
            var query = string.Format(System.Globalization.CultureInfo.InvariantCulture,
    "[out:json];node[\"amenity\"=\"fuel\"](around:{0},{1},{2});out;",
    radiusMeters, lat, lon);
            var url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);

            _logger.LogInformation("Overpass URL: {Url}", url);

            try
            {
                var response = await _http.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();  

                _logger.LogInformation("Status: {0}, Response: {1}", response.StatusCode, content);

                _logger.LogInformation("Overpass status: {Code}", response.StatusCode);
                _logger.LogInformation("Overpass response: {Content}", content);

                response.EnsureSuccessStatusCode(); // Will throw if not 200

                var result = JsonSerializer.Deserialize<OverpassResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var stations = result?.Elements
                    .Where(e => e.Type == "node")
                    .Select(e => new FuelStationDto
                    {
                        Name = e.Tags?.GetValueOrDefault("name") ?? "Unnamed",
                        Latitude = e.Lat,
                        Longitude = e.Lon
                    })
                    .ToList() ?? new();

                return stations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query Overpass API");
                return new();
            }
        }

        private class OverpassResponse
        {
            public List<Element> Elements { get; set; } = new();

            public class Element
            {
                public string Type
                {
                    get; set;
                }
                public double Lat
                {
                    get; set;
                }
                public double Lon
                {
                    get; set;
                }
                public Dictionary<string, string>? Tags
                {
                    get; set;
                }
            }
        }
    }
}
