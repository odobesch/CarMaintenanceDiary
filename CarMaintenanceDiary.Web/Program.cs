using BlazorBootstrap;
using Blazored.Toast;
using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Web.Components;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddBlazorBootstrap();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IVehicleService, VehicleApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7260/");
});

builder.Services.AddHttpClient<IFuelService, FuelApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7260/");
});

builder.Services.AddScoped<FuelStationService>();
builder.Services.AddSingleton<ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();