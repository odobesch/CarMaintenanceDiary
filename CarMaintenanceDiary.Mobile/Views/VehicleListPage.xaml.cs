using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Mobile.ViewModels;
using CarMaintenanceDiary.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CarMaintenanceDiary.Mobile.Views;

public partial class VehicleListPage : ContentPage
{
    private readonly VehicleListViewModel _viewModel;    

    public VehicleListPage(VehicleListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadVehiclesAsync();
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is VehicleDto selectedVehicle)
            {
                await Shell.Current.GoToAsync($"{nameof(VehicleDetailPage)}?VehicleId={selectedVehicle.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to vehicle detail page: {ex.Message}");
        }
    }
}