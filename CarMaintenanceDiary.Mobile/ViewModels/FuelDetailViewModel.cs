using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Mobile.ViewModels
{   
    public partial class FuelDetailViewModel : ObservableObject
    {
        private readonly IFuelService _fuelService;

        [ObservableProperty]
        private FuelRecordDto fuelRecord;

        public int FuelRecordId
        {
            get; set;
        }

        public FuelDetailViewModel(IFuelService fuelService)
        {
            _fuelService = fuelService;
        }

        public async Task LoadFuelRecordAsync()
        {
            try
            {
                FuelRecord = await _fuelService.GetFuelRecordByIdAsync(FuelRecordId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
