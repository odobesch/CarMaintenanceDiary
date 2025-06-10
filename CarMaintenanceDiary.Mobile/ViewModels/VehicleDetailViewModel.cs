using CarMaintenanceDiary.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CarMaintenanceDiary.Mobile.ViewModels
{
    public partial class VehicleDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private VehicleDto vehicle;

        public VehicleDetailViewModel(VehicleDto vehicle)
        {
            this.vehicle = vehicle;
        }
    }
}
