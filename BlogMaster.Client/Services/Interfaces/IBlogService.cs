using BlogMaster.Shared.Models;

namespace BlogMaster.Client.Services.Interfaces
{
    public interface IBlogService
    {
        Task SaveBlogPost(BlogDto blog);
        Task<BlogDto> GetBlogPost(int id);
        Task<List<BlogDto>> GetBlogPosts();
        Task AddComment(int Id, CommentDto comment);
    }
}
