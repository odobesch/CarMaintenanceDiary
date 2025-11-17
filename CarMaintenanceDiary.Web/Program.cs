using BlazorBootstrap;
using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Web.Components;
using CarMaintenanceDiary.Web.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Components.Authorization;

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

builder.Services.AddScoped<FuelStationService>();
builder.Services.AddSingleton<ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Error", createScopeForErrors: true);
 app.UseHsts();
}

app.UseHttpsRedirection();

// IMPORTANT: routing + auth + antiforgery middleware must run before endpoint mapping
app.UseRouting();

// If you have authentication/authorization in your app, ensure middleware is present.
// Calling these even if no handlers configured is safe; place UseAntiforgery after them.
app.UseAuthentication();
app.UseAuthorization();

// Add antiforgery middleware so endpoints with antiforgery metadata work
app.UseAntiforgery();

// If you have static assets mapping
app.MapStaticAssets();

// Map Razor Pages (for /Account/Logout)
app.MapRazorPages();

app.MapRazorComponents<CarMaintenanceDiary.Web.Components.App>()
 .AddInteractiveServerRenderMode();

app.Run();