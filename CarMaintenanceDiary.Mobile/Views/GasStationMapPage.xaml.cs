using CarMaintenanceDiary.Application.Services;
using System.Globalization;
using System.Text.Json;

namespace CarMaintenanceDiary.Mobile.Views;

public partial class GasStationMapPage : ContentPage
{
    private readonly FuelStationService _fuelStationService;
    private bool _mapInitialized = false;
    public GasStationMapPage(FuelStationService fuelStationService)
    {
        InitializeComponent();
        _fuelStationService = fuelStationService;

        var htmlPath = Path.Combine(FileSystem.AppDataDirectory, "osm-map.html"); // Updated to use AppDataDirectory
        MapWebView.Source = "osm-map.html";

        MapWebView.Navigated += async (s, e) =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Map HTML loaded. Getting location...");

                var location = await Geolocation.GetLastKnownLocationAsync()
                    ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

                double lat = location?.Latitude ?? 50.0755;
                double lng = location?.Longitude ?? 14.4378;

                await MapWebView.EvaluateJavaScriptAsync($"initMap({lat.ToString(CultureInfo.InvariantCulture)}, {lng.ToString(CultureInfo.InvariantCulture)})");

                _mapInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get location or init map: {ex}");
            }
        };
    }

    protected override async void OnAppearing()
    {        
        base.OnAppearing();

        if (_mapInitialized)
        {
            try
            {
                await LoadStationsAndShowOnMap();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnAppearing Exception: {ex}");
            }
        }
    }

    private async Task LoadStationsAndShowOnMap()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            double lat = location?.Latitude ?? 50.0755;
            double lng = location?.Longitude ?? 14.4378;

            var radiusMeters = 5000;
            var stations = await _fuelStationService.GetNearbyFuelStationsAsync(lat, lng, radiusMeters);

            System.Diagnostics.Debug.WriteLine($"Sending {stations.Count} stations to JS");

            var json = JsonSerializer.Serialize(stations);

            await MapWebView.EvaluateJavaScriptAsync($"addGasStations({json})");
        }
        catch (Exception)
        {

        }
    }
}
