using BlogMaster.Components;
using BlogMaster.Services.Implementations;
using BlogMaster.Services.Interfaces;
using BlogMaster.Shared.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddTransient<IBlogSqlService, BlogSqlService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapGet("/api/blogs", async (IBlogSqlService blogService) =>
{
    var allPosts = await blogService.GetBlogs();
    return Results.Ok(allPosts);
});

app.MapGet("/api/blogs/{id}", async (IBlogSqlService blogService, int id) =>
{
    var post = await blogService.GetBlog(id);
    return post is not null ? Results.Ok(post) : Results.NotFound();
});

app.MapPost("/api/blogs", async (IBlogSqlService blogService, [FromBody] BlogDto dto) =>
{
    await blogService.CreateBlog(dto);
    return Results.Created($"/api/blogs/{dto.Id}", dto);
});

app.MapPut("/api/blogs", async (IBlogSqlService blogService, [FromBody] BlogDto dto) =>
{
    await blogService.UpdateBlog(dto);
    return Results.NoContent(); // 204 No Content is typical for successful PUT
});

app.MapDelete("/api/blogs/{id}", async (IBlogSqlService blogService, int id) =>
{
    var result = await blogService.DeleteBlog(id);
    return result ? Results.NoContent() : Results.NotFound();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlogMaster.Client._Imports).Assembly);

await app.RunAsync();
