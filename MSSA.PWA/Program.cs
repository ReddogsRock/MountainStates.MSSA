using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MSSA.PWA;
using MSSA.PWA.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for API calls to Oqtane
var oqtaneBaseUrl = builder.Configuration["OqtaneApiUrl"] ?? "https://localhost:44329";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(oqtaneBaseUrl)
});

// Register PWA services
builder.Services.AddScoped<IOfflineStorageService, OfflineStorageService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IApiService, ApiService>();

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

await builder.Build().RunAsync();
