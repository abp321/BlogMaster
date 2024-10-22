using BlogMaster.Client.Services.Implementations;
using BlogMaster.Shared.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddHttpClient<IBlogService, BlogClientService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
await builder.Build().RunAsync();
