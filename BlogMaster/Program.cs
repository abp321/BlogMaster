using BlogMaster.Components;
using BlogMaster.Database;
using BlogMaster.Models;
using BlogMaster.Services.Implementations;
using BlogMaster.Services.Interfaces;
using BlogMaster.Shared.Implementations;
using BlogMaster.Shared.Interfaces;
using BlogMaster.Shared.Models;
using BlogMaster;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
AppSettings appSettings = await InitializeAppSettings(builder);
ConfigureServices(builder, appSettings);

var app = builder.Build();
ConfigureMiddleware(app);
ConfigureEndpoints(app);

await app.RunAsync();

async Task<AppSettings> InitializeAppSettings(WebApplicationBuilder builder)
{
    AppSettings appSettings = new();
    builder.Configuration.GetSection(nameof(AppSettings)).Bind(appSettings);
    appSettings.SqlConnectionString = await Scripts.InitializeSQLiteDatabase();//Use sql connection string from config in 
    return appSettings;
}

void ConfigureServices(WebApplicationBuilder builder, AppSettings appSettings)
{
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [
            "application/octet-stream",
            "application/javascript",
            "application/wasm",
            "text/css",
            "text/html",
            "application/json",
            "font/woff",
            "font/woff2",
            "image/svg+xml"
        ]);
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });

    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseSqlite(appSettings.SqlConnectionString));

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddScoped<IBlogSqlService, BlogSqlService>();
    builder.Services.AddScoped<IBlogService, BlogService>();
    builder.Services.AddScoped<IWebAssemblyStateCacheService<BlogDto>, WebAssemblyStateCacheService<BlogDto>>();
    builder.Services.AddScoped<IWebAssemblyStateCacheService<List<BlogDto>>, WebAssemblyStateCacheService<List<BlogDto>>>();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        },
        
    });
    app.UseAntiforgery();
}

void ConfigureEndpoints(WebApplication app)
{
    app.MapGet("/api/blogs", async (IBlogService blogService) =>
    {
        var allBlogs = await blogService.GetBlogs();
        return Results.Ok(allBlogs);
    });

    app.MapGet("/api/blogs/{id}", async (IBlogService blogService, int id) =>
    {
        var blog = await blogService.GetBlog(id);
        return blog is not null ? Results.Ok(blog) : Results.NotFound();
    });

    app.MapPost("/api/blogs", async (IBlogService blogService, [FromBody] BlogDto dto) =>
    {
        var createdBlog = await blogService.CreateBlog(dto);
        return createdBlog is not null
            ? Results.Created($"/api/blogs/{createdBlog.Id}", createdBlog)
            : Results.BadRequest("Blog creation failed.");
    });

    app.MapPut("/api/blogs", async (IBlogService blogService, [FromBody] BlogDto dto) =>
    {
        var updatedBlog = await blogService.UpdateBlog(dto);
        return updatedBlog is not null
            ? Results.Ok(updatedBlog)
            : Results.NotFound("Blog not found or update failed.");
    });

    app.MapDelete("/api/blogs/{id}", async (IBlogService blogService, int id) =>
    {
        var deleted = await blogService.DeleteBlog(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    });

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(BlogMaster.Client._Imports).Assembly);
}
