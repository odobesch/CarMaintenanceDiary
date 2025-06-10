using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CarMaintenanceDiary.Mobile.ViewModels;
using CarMaintenanceDiary.Mobile.Views;
using FluentValidation;
using CarMaintenanceDiary.Mobile.Validators;
using Plugin.Maui.OCR;

namespace CarMaintenanceDiary.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMarkup()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fa-solid-900.ttf", "FAS");
                });


            builder.Services.AddHttpClient<IVehicleService, VehicleApiService>(client =>
            {
                client.BaseAddress = new Uri("https://zjs3ddk7-7260.euw.devtunnels.ms/"); //Create your own dev tunnel for testing
            });

            builder.Services.AddHttpClient<IFuelService, FuelApiService>(client =>
            {
                client.BaseAddress = new Uri("https://zjs3ddk7-7260.euw.devtunnels.ms/");
            });

            builder.Services.AddSingleton<IOcrService>(OcrPlugin.Default);

            builder.Services.AddSingleton<VehicleListViewModel>();
            builder.Services.AddSingleton<VehicleListPage>();

            builder.Services.AddSingleton<VehicleDetailViewModel>();
            builder.Services.AddSingleton<VehicleDetailPage>();

            builder.Services.AddSingleton<VehicleAddViewModel>();
            builder.Services.AddSingleton<VehicleAddPage>();
            builder.Services.AddTransient<IValidator<VehicleAddViewModel>, VehicleAddViewModelValidator>();

            builder.Services.AddSingleton<VehicleEditViewModel>();
            builder.Services.AddSingleton<VehicleEditPage>();

            builder.Services.AddSingleton<FuelListPage>();
            builder.Services.AddSingleton<FuelListViewModel>();

            builder.Services.AddSingleton<FuelDetailViewModel>();
            builder.Services.AddSingleton<FuelDetailPage>();

            builder.Services.AddHttpClient("FuelStation", client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MAUIApp/1.0)");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.ExpectContinue = false; 
            })
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true,
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
    UseCookies = false,             
    UseDefaultCredentials = false,  
    Proxy = null                    
});

            builder.Services.AddSingleton<FuelStationService>();
            builder.Services.AddSingleton<GasStationMapPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
