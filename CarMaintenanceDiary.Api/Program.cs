using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Application.Services;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Infrastructure.Email;
using CarMaintenanceDiary.Infrastructure.Identity;
using CarMaintenanceDiary.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using static CarMaintenanceDiary.Infrastructure.Email.SmtpEmailSender;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddScoped<IVehicleService, VehicleApiService>();
builder.Services.AddScoped<IFuelService, FuelApiService>();
builder.Services.AddScoped<IUserManagementService, UserManagementApiService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
});

var mailjetSection = builder.Configuration.GetSection("Mailjet");
var mailjetApiKey = mailjetSection["ApiKey"];
var mailjetApiSecret = mailjetSection["ApiSecret"];

var mailtrapApiToken = builder.Configuration.GetValue<string>("Mailtrap:ApiToken");
if (!string.IsNullOrWhiteSpace(mailtrapApiToken))
{
    builder.Services.Configure<MailtrapOptions>(builder.Configuration.GetSection("Mailtrap"));
    builder.Services.AddTransient<IEmailSender, MailtrapEmailSender>();
}
else if (!string.IsNullOrWhiteSpace(mailjetApiKey) && !string.IsNullOrWhiteSpace(mailjetApiSecret))
{
    builder.Services.Configure<MailjetOptions>(mailjetSection);
    builder.Services.AddSingleton<IValidateOptions<MailjetOptions>, MailjetOptionsValidator>();
    builder.Services.AddTransient<IEmailSender, MailjetEmailSender>();
}
else
{
    builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
    builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
}

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
