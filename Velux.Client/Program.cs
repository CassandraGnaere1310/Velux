using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Velux.Client;
using Velux.Client.ApiClients;
using Velux.Client.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The API runs separately; its address is in wwwroot/appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Model layer: API clients (ViewModels depend on the interfaces)
builder.Services.AddScoped<ISupplierApiClient, SupplierApiClient>();
builder.Services.AddScoped<IComparisonApiClient, ComparisonApiClient>();

// ViewModel layer
builder.Services.AddTransient<SupplierDataViewModel>();
builder.Services.AddTransient<SupplierComparisonViewModel>();

await builder.Build().RunAsync();
