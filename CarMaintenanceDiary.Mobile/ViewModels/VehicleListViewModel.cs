using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Mobile.Views;
using CarMaintenanceDiary.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CarMaintenanceDiary.Mobile.ViewModels
{
    public partial class VehicleListViewModel : ObservableObject
    {
        private readonly IVehicleService _vehicleService;

        public VehicleListViewModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [ObservableProperty]
        private ObservableCollection<VehicleDto> vehicles = new();
        
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;

        [RelayCommand]
        public async Task LoadVehiclesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;            

            try
            {
                Vehicles.Clear();
                var items = await _vehicleService.GetAllAsync();
                foreach (var item in items)
                {
                    Vehicles.Add(item);                
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., show an alert)
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load vehicles: {ex.Message}", "OK");
                Console.WriteLine($"Error loading vehicles: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OnRefreshing()
        {
            IsRefreshing = true;

            try
            {
                await LoadVehiclesAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task ShowVehicleDetailAsync(VehicleDto vehicle)
        {
            if (vehicle != null)
            {                
                await Shell.Current.GoToAsync($"VehicleDetailPage?VehicleId={vehicle.Id}");
            }
        }

        [RelayCommand]
        private async Task ShowFuelRecordsAsync(VehicleDto vehicle)
        {
            if (vehicle != null)
            {
                await Shell.Current.GoToAsync($"FuelListPage?vehicleId={vehicle.Id}");
            }                
        }

        [RelayCommand]
        private async Task AddVehicleAsync()
        {
            await Shell.Current.GoToAsync(nameof(VehicleAddPage));
        }

        [RelayCommand]
        private async Task EditVehicleAsync(VehicleDto vehicle)
        {
            if (vehicle is null) return;

            var viewModel = new VehicleEditViewModel(vehicle, _vehicleService);
            var editPage = new VehicleEditPage(viewModel);

            await Shell.Current.Navigation.PushAsync(editPage);
        }

        [RelayCommand]
        private async Task DeleteVehicleAsync(VehicleDto vehicle)
        {
            if (vehicle == null)
                return;

            // Show confirmation dialog
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the vehicle '{vehicle.Make} {vehicle.Model}'?",
                "Yes",
                "No");

            if (!confirm)
                return;

            IsBusy = true;
            try
            {
                await _vehicleService.DeleteAsync(vehicle.Id);
                Vehicles.Remove(vehicle); // Remove from the local collection
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to delete vehicle: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }        
    }
}
