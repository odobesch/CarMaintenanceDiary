using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Shared.DTOs;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Mobile.ViewModels
{
    public partial class FuelListViewModel : ObservableObject
    {
        private readonly IFuelService _fuelService;
        public int VehicleId
        {
            get; set;
        }

        [ObservableProperty]
        private ObservableCollection<FuelRecordDto> fuelRecords = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;       

        public FuelListViewModel(IFuelService fuelService)
        {
            _fuelService = fuelService;
        }

        [RelayCommand]
        public async Task LoadFuelRecordsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                FuelRecords.Clear();
                var items = await _fuelService.GetFuelRecordsAsync(VehicleId);
                foreach (var item in items)
                {
                    FuelRecords.Add(item);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load fuel records: {ex.Message}", "OK");
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
                await LoadFuelRecordsAsync();
            }
            finally { IsRefreshing = false; }
        }

        [RelayCommand]
        private async Task ShowFuelDetailAsync(FuelRecordDto fuelRecord)
        {
            try
            {
                if (fuelRecord != null)
                    await Shell.Current.GoToAsync($"FuelDetailPage?fuelRecordId={fuelRecord.Id}");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /*[RelayCommand]
        private async Task AddFuelRecordAsync()
        {
            await Shell.Current.GoToAsync(nameof());
        }*/

        /*[RelayCommand]
        private async Task EditVehicleAsync(VehicleDto vehicle)
        {
            if (vehicle == null)
                return;

            var viewModel = new VehicleEditViewModel(vehicle, _vehicleService);
            var editPage = new VehicleEditPage(viewModel);

            await Shell.Current.Navigation.PushAsync(editPage);
        }*/

        /*[RelayCommand]
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
        }*/
    }
}
