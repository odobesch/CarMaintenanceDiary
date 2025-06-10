using CarMaintenanceDiary.Mobile.ViewModels;
using Plugin.Maui.OCR;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Mobile.Views;

public partial class VehicleAddPage : ContentPage
{
	public VehicleAddPage(VehicleAddViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;		
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing(); 

        await OcrPlugin.Default.InitAsync();
    }
}