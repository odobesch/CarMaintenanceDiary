using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Mobile.ViewModels;
using CarMaintenanceDiary.Shared.DTOs;

namespace CarMaintenanceDiary.Mobile.Views;

public partial class VehicleEditPage : ContentPage
{
    public VehicleDto Vehicle
    {
        get => (BindingContext as VehicleEditViewModel)?.Vehicle;
        set
        {
            if (BindingContext is VehicleEditViewModel viewModel)
            {
                viewModel.Vehicle = value;
                viewModel.Make = value.Make;
                viewModel.Model = value.Model;
                viewModel.Year = value.Year;
                viewModel.LicensePlate = value.LicensePlate;
                viewModel.Vin = value.VIN;
            }
        }
    }

    public VehicleEditPage(VehicleEditViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}