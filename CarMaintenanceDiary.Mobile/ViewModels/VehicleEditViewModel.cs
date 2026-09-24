using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarMaintenanceDiary.Mobile.ViewModels
{
    public partial class VehicleEditViewModel : ObservableObject
    {
        private readonly IVehicleService _vehicleService;

        [ObservableProperty]
        private VehicleDto vehicle;

        [ObservableProperty]
        private string make;

        [ObservableProperty]
        private string model;

        [ObservableProperty]
        private string year;

        [ObservableProperty]
        private string licensePlate;

        [ObservableProperty]
        private string vin;

        [ObservableProperty]
        private bool isBusy;

        public VehicleEditViewModel(VehicleDto vehicle, IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
            Vehicle = vehicle;
            Make = vehicle.Make;
            Model = vehicle.Model;
            Year = vehicle.Year;
            LicensePlate = vehicle.LicensePlate;
            Vin = vehicle.VIN;
        }

        [RelayCommand]
        private async Task EditVehicleAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await _vehicleService.UpdateAsync(Vehicle);
                await Shell.Current.GoToAsync(".."); // Navigate back
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
