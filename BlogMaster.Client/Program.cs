using BlogMaster.Client.Services.Implementations;
using BlogMaster.Shared.Implementations;
using BlogMaster.Shared.Interfaces;
using BlogMaster.Shared.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped<IWebAssemblyStateCacheService<BlogDto>, WebAssemblyStateCacheService<BlogDto>>();
builder.Services.AddScoped<IWebAssemblyStateCacheService<List<BlogDto>>, WebAssemblyStateCacheService<List<BlogDto>>>();
builder.Services.AddHttpClient<IBlogService, BlogClientService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
await builder.Build().RunAsync();
