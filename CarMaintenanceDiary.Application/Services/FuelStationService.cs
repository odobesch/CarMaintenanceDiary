using CarMaintenanceDiary.Shared.DTOs;
using System.Net.Http;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services
{
    public class FuelStationService
    {
        private readonly HttpClient _http;

        public FuelStationService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
        }

        public async Task<List<FuelStationDto>> GetNearbyFuelStationsAsync(double lat, double lon, int radiusMeters = 5000)
        {
            var query = $"""
        [out:json];
        node["amenity"="fuel"](around:{radiusMeters},{lat},{lon});
        out;
        """;

            var url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);

            var result = await _http.GetFromJsonAsync<OverpassResponse>(url);

            return result?.Elements
                .Where(e => e.Type == "node")
                .Select(e => new FuelStationDto
                {
                    Name = e.Tags?.GetValueOrDefault("name") ?? "Unnamed",
                    Latitude = e.Lat,
                    Longitude = e.Lon
                }).ToList() ?? new();
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
