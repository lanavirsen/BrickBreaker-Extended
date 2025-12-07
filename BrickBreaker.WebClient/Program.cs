using BrickBreaker.Core.Clients;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BrickBreaker.WebClient;
using BrickBreaker.WebClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var preferredBase = builder.Configuration["ApiBaseUrl"];
var apiBase = ApiConfiguration.ResolveBaseAddress(preferredBase);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase, UriKind.Absolute) });
builder.Services.AddScoped<ApiClient>();
var turnstileSection = builder.Configuration.GetSection("Turnstile");
builder.Services.Configure<TurnstileClientOptions>(options => turnstileSection.Bind(options));

await builder.Build().RunAsync();
