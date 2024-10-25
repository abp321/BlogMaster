using BlogMaster.Models.Entities;

namespace BlogMaster.Services.Interfaces
{
    public interface IBlogSqlService
    {
        Task<BlogEntity?> GetBlog(int id);
        Task<List<BlogEntity>> GetBlogs();
        Task<BlogEntity?> CreateBlog(BlogEntity blog);
        Task<BlogEntity?> UpdateBlog(BlogEntity blog);
        Task<bool> DeleteBlog(int id);
        IAsyncEnumerable<VisitorEntity> GetVisitors();
    }
}
