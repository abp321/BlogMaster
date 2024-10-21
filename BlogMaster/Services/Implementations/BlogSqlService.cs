using BlogMaster.Services.Interfaces;
using BlogMaster.Shared.Models;

namespace BlogMaster.Services.Implementations
{
    public class BlogSqlService : IBlogSqlService
    {
        public Task<BlogDto> GetBlog(int id)
        {
            return Task.FromResult(new BlogDto());
        }

        public Task<List<BlogDto>> GetBlogs()
        {
            return Task.FromResult(new List<BlogDto>());
        }

        public Task<bool> CreateBlog(BlogDto dto)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateBlog(BlogDto dto)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteBlog(int id)
        {
            return Task.FromResult(true);
        }
    }
}
