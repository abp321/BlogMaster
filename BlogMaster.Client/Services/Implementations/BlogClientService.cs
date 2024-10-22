using BlogMaster.Shared.Interfaces;
using BlogMaster.Shared.Models;
using System.Net.Http.Json;

namespace BlogMaster.Client.Services.Implementations
{
    public class BlogClientService(HttpClient httpClient) : IBlogService
    {
        public Task<List<BlogDto>?> GetBlogs()
        {
            return httpClient.GetFromJsonAsync<List<BlogDto>>("api/blogs");
        }

        public Task<BlogDto?> GetBlog(int id)
        {
            return httpClient.GetFromJsonAsync<BlogDto>($"api/blogs/{id}");
        }

        public async Task<BlogDto?> CreateBlog(BlogDto dto)
        {
            var response = await httpClient.PostAsJsonAsync("api/blogs", dto);
            //use response.Headers.Location from the created response later if needed
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BlogDto>();
            }
            else
            {
                return null;
            }
        }

        public async Task<BlogDto?> UpdateBlog(BlogDto dto)
        {
            var response = await httpClient.PutAsJsonAsync("api/blogs", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BlogDto>();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteBlog(int id)
        {
            var response = await httpClient.DeleteAsync($"api/blogs/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
