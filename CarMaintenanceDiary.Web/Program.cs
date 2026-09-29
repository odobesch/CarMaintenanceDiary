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

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("Configuration value 'ApiBaseUrl' is not set.");

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Named API client
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
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

builder.Services.AddHttpClient("FuelStation", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; CarMaintenanceDiaryWeb/1.0)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    // overpass-api.de uses Apache content negotiation and returns 406 Not Acceptable
    // to requests that don't include an Accept-Language header.
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
});

builder.Services.AddScoped<FuelStationService>();
builder.Services.AddScoped<ToastService>();

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