using CarMaintenanceDiary.Mobile.Views;

namespace CarMaintenanceDiary.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(VehicleListPage), typeof(VehicleListPage));
            Routing.RegisterRoute(nameof(VehicleDetailPage), typeof(VehicleDetailPage));
            Routing.RegisterRoute(nameof(VehicleAddPage), typeof(VehicleAddPage));
            Routing.RegisterRoute(nameof(VehicleEditPage), typeof(VehicleEditPage));
            Routing.RegisterRoute(nameof(FuelListPage), typeof(FuelListPage));
            Routing.RegisterRoute(nameof(FuelDetailPage), typeof(FuelDetailPage));
        }
    }
}
