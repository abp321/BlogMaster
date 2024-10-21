using BlogMaster.Client.Services.Interfaces;
using BlogMaster.Shared.Models;

namespace BlogMaster.Client.Services.Implementations
{
    public class BlogService : IBlogService
    {
        public Task<List<BlogDto>> GetBlogPosts()
        {
            return Task.FromResult(new List<BlogDto>());
        }

        public Task<BlogDto> GetBlogPost(int id)
        {
            return Task.FromResult(new BlogDto());
        }

        public Task SaveBlogPost(BlogDto blog)
        {
            return Task.FromResult(new BlogDto());
        }

        public Task AddComment(int Id, CommentDto comment)
        {
            return Task.CompletedTask;
        }
    }
}
