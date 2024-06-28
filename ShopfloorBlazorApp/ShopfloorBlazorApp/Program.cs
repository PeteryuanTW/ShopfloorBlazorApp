using ShopfloorBlazorApp.Components;
using ShopfloorBlazorApp.Data;
using Microsoft.EntityFrameworkCore;
using ShopfloorBlazorApp.EFModels;
using ShopfloorBlazorApp.Service;
using CommonLibrary.UIPack;
using Serilog;
using Serilog.Filters;
using Blazored.Toast;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddLocalization();

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File("Logs/Service/ServiceLog.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, shared: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
//    .CreateLogger();
//builder.Services.AddSerilog();//log all info from ms
//builder.Host.UseSerilog();

builder.Services.AddDevExpressBlazor(options =>
{
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});
builder.Services.AddDbContextFactory<ShopfloorServiceDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<ShopfloorServiceDataService>();
builder.Services.AddSingleton<UIService>();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<EditMode>();

builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<ShopfloorServiceSignalRClient>();
var app = builder.Build();



ShopfloorServiceDataService shopfloorServiceDataService = app.Services.GetRequiredService<ShopfloorServiceDataService>();
string langStr = await shopfloorServiceDataService.GetLanguage();
app.UseRequestLocalization(langStr);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
//.AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();