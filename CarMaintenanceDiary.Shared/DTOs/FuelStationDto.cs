using System.Text.Json.Serialization;

namespace CarMaintenanceDiary.Shared.DTOs
{
    public class FuelStationDto
    {
       [JsonPropertyName("name")]
        public string Name { get; set; } = "Unnamed";

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
