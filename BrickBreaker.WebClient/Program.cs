using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BrickBreaker.WebClient;
using BrickBreaker.WebClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://127.0.0.1:5080";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase, UriKind.Absolute) });
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
