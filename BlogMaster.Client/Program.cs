using BlogMaster.Client.Services.Implementations;
using BlogMaster.Client.Services.Interfaces;
using BlogMaster.Shared.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IWebAssemblyStateCacheService, WebAssemblyStateCacheService>();

builder.Services.AddHttpClient<IBlogService, BlogClientService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddStandardResilienceHandler();
await builder.Build().RunAsync();
