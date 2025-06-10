using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Mobile.ViewModels;
using CarMaintenanceDiary.Shared.DTOs;

namespace CarMaintenanceDiary.Mobile.Views;

[QueryProperty(nameof(VehicleId), "VehicleId")]
public partial class VehicleDetailPage : ContentPage
{
    private readonly IVehicleService _vehicleService;

    public int VehicleId
    {
        get => (int)GetValue(VehicleIdProperty);
        set
        {
            SetValue(VehicleIdProperty, value);
            LoadVehicle(value);
        }
    }

    public static readonly BindableProperty VehicleIdProperty =
        BindableProperty.Create(nameof(VehicleId), typeof(int), typeof(VehicleDetailPage), 0);

    public VehicleDetailPage(IVehicleService vehicleService)
    {
        InitializeComponent();
        _vehicleService = vehicleService;
    }

    private async void LoadVehicle(int id)
    {
        VehicleDto vehicle = new VehicleDto();
       
        try
        {
            vehicle = await _vehicleService.GetByIdAsync(id); 
        }
        catch (Exception)
        {
            
        }
        
        BindingContext = new VehicleDetailViewModel(vehicle);
    }
}