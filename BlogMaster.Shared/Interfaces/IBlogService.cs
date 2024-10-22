using BlogMaster.Shared.Models;

namespace BlogMaster.Shared.Interfaces
{
    public interface IBlogService
    {
        Task<BlogDto?> GetBlog(int id);
        Task<List<BlogDto>?> GetBlogs();
        Task<BlogDto?> CreateBlog(BlogDto dto);
        Task<BlogDto?> UpdateBlog(BlogDto dto);
        Task<bool> DeleteBlog(int id);
    }
}
