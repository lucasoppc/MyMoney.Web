using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyMoney.Api.Sdk.Configuration;
using MyMoney.Web;
using MyMoney.Web.Filters;
using MyMoney.Web.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMemoryCache();

builder.Services.AddScoped<CacheService>();
builder.Services.AddMyMoneyApiSdk(new MyMoneyApiSdkOptions()
{
    BaseUrl = "https://oppc-my-money-api.azurewebsites.net",
    TimeoutInSeconds = 30,
});

var app = builder.Build();


await app.RunAsync();