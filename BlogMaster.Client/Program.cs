using BlogMaster.Client.Services.Implementations;
using BlogMaster.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped<IBlogService, BlogService>();
await builder.Build().RunAsync();
