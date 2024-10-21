using BlogMaster.Shared.Models;

namespace BlogMaster.Services.Interfaces
{
    public interface IBlogSqlService
    {
        public Task<List<BlogDto>> GetBlogs();
        public Task<BlogDto> GetBlog(int id);
        public Task<bool> CreateBlog(BlogDto dto);
        public Task<bool> UpdateBlog(BlogDto dto);
        public Task<bool> DeleteBlog(int id);
    }
}
