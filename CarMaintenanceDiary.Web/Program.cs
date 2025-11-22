using BlazorBootstrap;
using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
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

builder.Services.AddRazorPages();

builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenStore, InMemoryTokenStore>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddTransient<ApiAuthHttpHandler>();

// Named API client for auth and other calls
builder.Services.AddHttpClient("Api", client =>
{
 client.BaseAddress = new Uri("https://localhost:7260/");
}).AddHttpMessageHandler<ApiAuthHttpHandler>();

// Http clients with auth handler using the named Api client
builder.Services.AddHttpClient<IVehicleService, VehicleApiService>(client =>
{
 client.BaseAddress = new Uri("https://localhost:7260/");
}).AddHttpMessageHandler<ApiAuthHttpHandler>();

builder.Services.AddHttpClient<IFuelService, FuelApiService>(client =>
{
 client.BaseAddress = new Uri("https://localhost:7260/");
}).AddHttpMessageHandler<ApiAuthHttpHandler>();

builder.Services.AddHttpClient<IMaintenanceService, MaintenanceApiService>(client =>
{
 client.BaseAddress = new Uri("https://localhost:7260/");
}).AddHttpMessageHandler<ApiAuthHttpHandler>();

builder.Services.AddHttpClient<IUserManagementService, UserManagementApiService>(client =>
{
 client.BaseAddress = new Uri("https://localhost:7260/");
}).AddHttpMessageHandler<ApiAuthHttpHandler>();

builder.Services.AddScoped<FuelStationService>();
builder.Services.AddSingleton<ToastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Error", createScopeForErrors: true);
 app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// If you have static assets mapping
app.MapStaticAssets();

app.MapRazorPages();

app.MapRazorComponents<CarMaintenanceDiary.Web.Components.App>()
 .AddInteractiveServerRenderMode();

app.Run();