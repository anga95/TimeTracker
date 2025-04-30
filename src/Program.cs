using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Services;
using TimeTracker.ViewModels;

var builder = WebApplication.CreateBuilder(args);

#region Configuration & Key Vault

var keyVaultUrl = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential()
    );
}

#endregion

#region Add core framework services

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

#endregion

#region Entity Framework Core & SQL Server

// register DbContext with retry logic for transient errors
builder.Services.AddDbContext<TimeTrackerContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ),
    ServiceLifetime.Scoped,
    ServiceLifetime.Singleton
);

builder.Services.AddDbContextFactory<TimeTrackerContext>();

#endregion

#region Identity & autentisering

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<TimeTrackerContext>();

#endregion

#region Applikationstjänster

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<AiSummaryStateService>();
builder.Services.AddScoped<ITimeTrackingService, TimeTrackingService>();

#endregion
#region ViewModels

builder.Services.AddScoped<TimeEntryViewModel>();
builder.Services.AddScoped<CalendarGridViewModel>();
builder.Services.AddScoped<MonthNavigationViewModel>();
builder.Services.AddScoped<DayDetailViewModel>();
builder.Services.AddScoped<ProjectSelectorViewModel>();
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddScoped<SafeExecutor>();

#endregion

#region Logging & Telemetry

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddApplicationInsightsTelemetry();

#endregion

var app = builder.Build();

#region Middleware-pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

#endregion

app.Run();