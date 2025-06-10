using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Mobile.ViewModels;
using CarMaintenanceDiary.Shared.DTOs.Fuel;

namespace CarMaintenanceDiary.Mobile.Views;

[QueryProperty(nameof(FuelRecordId), "fuelRecordId")]
public partial class FuelDetailPage : ContentPage
{
    public int FuelRecordId
    {
        get => (int)GetValue(FuelRecordIdProperty);
        set
        {
            SetValue(FuelRecordIdProperty, value);
            if (BindingContext is FuelDetailViewModel vm)
            {
                vm.FuelRecordId = value;
                _ = vm.LoadFuelRecordAsync(); // Fire and forget, or await if you prefer
            }
        }
    }

    public static readonly BindableProperty FuelRecordIdProperty =
        BindableProperty.Create(nameof(FuelRecordId), typeof(int), typeof(FuelDetailPage), 0);

    public FuelDetailPage(FuelDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}