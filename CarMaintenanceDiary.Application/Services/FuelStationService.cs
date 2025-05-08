using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class FuelStationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<FuelStationService> _logger;

        public FuelStationService(IHttpClientFactory factory, ILogger<FuelStationService> logger)
        {
            _http = factory.CreateClient();
            _logger = logger;
        }

        public async Task<List<FuelStationDto>> GetNearbyFuelStationsAsync(double lat, double lon, int radiusMeters = 5000)
        {
            var query = $"""
                [out:json];
                node["amenity"="fuel"](around:{radiusMeters},{lat},{lon});
                out;
                """;

            var url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);

            _logger.LogInformation("Requesting nearby fuel stations: lat={Latitude}, lon={Longitude}, radius={Radius}m", lat, lon, radiusMeters);

            try
            {
                var result = await _http.GetFromJsonAsync<OverpassResponse>(url);

                var stations = result?.Elements
                    .Where(e => e.Type == "node")
                    .Select(e => new FuelStationDto
                    {
                        Name = e.Tags?.GetValueOrDefault("name") ?? "Unnamed",
                        Latitude = e.Lat,
                        Longitude = e.Lon
                    })
                    .ToList() ?? new();

                _logger.LogInformation("Found {Count} stations", stations.Count);

                return stations;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while requesting fuel stations");
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetNearbyFuelStationsAsync");
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
