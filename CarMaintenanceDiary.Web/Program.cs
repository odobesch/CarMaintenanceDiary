using BlazorBootstrap;
using CarMaintenanceDiary.Application;
using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Web;
using CarMaintenanceDiary.Web.Authentication;
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

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddOutputCache();

// Register the token accessor
builder.Services.AddScoped<IAuthTokenAccessor, BlazorAuthTokenAccessor>();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7260/");
});

// Named API client
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7260/");
});

// Register services manually to ensure proper DI scope
builder.Services.AddScoped<IVehicleService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Api");
    var tokenAccessor = sp.GetRequiredService<IAuthTokenAccessor>();
    return new VehicleApiService(httpClient, tokenAccessor);
});

builder.Services.AddScoped<IUserManagementService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Api");
    var tokenAccessor = sp.GetRequiredService<IAuthTokenAccessor>();
    return new UserManagementApiService(httpClient, tokenAccessor);
});

builder.Services.AddScoped<IFuelService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Api");
    var tokenAccessor = sp.GetRequiredService<IAuthTokenAccessor>();
    return new FuelApiService(httpClient, tokenAccessor);
});

builder.Services.AddScoped<IMaintenanceService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Api");
    var tokenAccessor = sp.GetRequiredService<IAuthTokenAccessor>();
    return new MaintenanceApiService(httpClient, tokenAccessor);
});

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

app.UseOutputCache();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorPages();

app.MapControllers();

app.MapRazorComponents<CarMaintenanceDiary.Web.Components.App>()
 .AddInteractiveServerRenderMode();

app.Run();