using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Mobile.ViewModels;

namespace CarMaintenanceDiary.Mobile.Views;

[QueryProperty(nameof(VehicleId), "vehicleId")]
public partial class FuelListPage : ContentPage
{
    private readonly FuelListViewModel _viewModel;

    public int VehicleId
    {
        get => (int)GetValue(VehicleIdProperty);
        set
        {
            SetValue(VehicleIdProperty, value);
            // Assign VehicleId to VM, load records
            if (_viewModel != null)
            {
                _viewModel.VehicleId = value;
                _viewModel.LoadFuelRecordsAsync();
            }
        }
    }

    public static readonly BindableProperty VehicleIdProperty =
        BindableProperty.Create(nameof(VehicleId), typeof(int), typeof(FuelListPage), 0);

    public FuelListPage(FuelListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}