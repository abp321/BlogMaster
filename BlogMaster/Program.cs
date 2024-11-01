using BlogMaster.Components;
using BlogMaster.Database;
using BlogMaster.Models;
using BlogMaster.Services.Implementations;
using BlogMaster.Services.Interfaces;
using BlogMaster.Shared.Interfaces;
using BlogMaster.Shared.Models;
using BlogMaster;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using BlogMaster.Middleware;
using System.Text.Json;
using BlogMaster.Client.Services.Implementations;
using BlogMaster.Client.Services.Interfaces;
using System.Threading.RateLimiting;

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
    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseSqlite(appSettings.SqlConnectionString));

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddScoped<IBlogSqlService, BlogSqlService>();
    builder.Services.AddScoped<IBlogService, BlogService>();
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var user = context.User;
            bool authenticated = user.Identity?.IsAuthenticated ?? false;
            string? userId = authenticated ? user.Identity?.Name : context.Connection.RemoteIpAddress?.ToString();
            return RateLimitPartition.GetTokenBucketLimiter(userId ?? "anonymous", partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                ReplenishmentPeriod = TimeSpan.FromMilliseconds(500),
                TokensPerPeriod = 20,
                QueueProcessingOrder = QueueProcessingOrder.NewestFirst,
                QueueLimit = 5
            });
        });

        options.OnRejected = (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return ValueTask.CompletedTask;
        };
    });

    builder.Services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1024 * 1024 * 50; // 50 MB
        options.CompactionPercentage = 0.2; // Evicts 20% of cache entries when under memory pressure
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddScoped<IWebAssemblyStateCacheService, WebAssemblyStateCacheService>();
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

    app.UseRateLimiter();

    app.Use(async (context, next) =>
    {
        using var dbContext = context.RequestServices.GetRequiredService<BlogDbContext>();
        var customMiddleware = new CustomMiddleware(next, dbContext);
        await customMiddleware.InvokeAsync(context);
    });

    app.UseHttpsRedirection();
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

    app.MapGet("/api/blogs/limited", (HttpContext context) =>
    {
        return Results.Ok();
    });

    app.MapGet("/api/blogs/visitors", async (HttpContext context, IBlogSqlService blogSqlService) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("[");

        bool firstItem = true;

        await foreach (var visitor in blogSqlService.GetVisitors())
        {
            if (!firstItem)
            {
                await context.Response.WriteAsync(",");
            }
            else
            {
                firstItem = false;
            }
            await JsonSerializer.SerializeAsync(context.Response.Body, visitor);
        }
        await context.Response.WriteAsync("]");
    });


    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(BlogMaster.Client._Imports).Assembly);
}
