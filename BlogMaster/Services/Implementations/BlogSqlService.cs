using BlogMaster.Database;
using BlogMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BlogMaster.Models.Entities;

namespace BlogMaster.Services.Implementations
{
    public class BlogSqlService(IServiceProvider serviceProvider) : IBlogSqlService
    {
        public async Task<BlogEntity?> GetBlog(int id)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            return await dbContext.Blogs
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BlogEntity>> GetBlogs()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            return await dbContext.Blogs
                .Include(b => b.Comments)
                .ToListAsync();
        }

        public async Task<BlogEntity?> CreateBlog(BlogEntity blogEntity)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                blogEntity.PublishedDate = DateTime.UtcNow;
                dbContext.Blogs.Add(blogEntity);
                await dbContext.SaveChangesAsync();
                return blogEntity;
            }
            catch
            {
                return null;
            }
        }

        public async Task<BlogEntity?> UpdateBlog(BlogEntity blogEntity)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                var existingBlog = await dbContext.Blogs
                    .Include(b => b.Comments)
                    .FirstOrDefaultAsync(b => b.Id == blogEntity.Id);

                if (existingBlog is null) return null;

                dbContext.Entry(existingBlog).CurrentValues.SetValues(blogEntity);
                UpdateComments(dbContext, existingBlog, blogEntity.Comments);

                await dbContext.SaveChangesAsync();
                return await dbContext.Blogs
                    .Include(b => b.Comments)
                    .FirstOrDefaultAsync(b => b.Id == existingBlog.Id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteBlog(int id)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                var blog = await dbContext.Blogs.FindAsync(id);
                if (blog is null) return false;

                dbContext.Blogs.Remove(blog);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async IAsyncEnumerable<VisitorEntity> GetVisitors()
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            await foreach(VisitorEntity visitor in dbContext.Visitors.AsAsyncEnumerable())
            {
                yield return visitor;
            }
        }

        private void UpdateComments(BlogDbContext context, BlogEntity existingBlog, List<CommentEntity> updatedComments)
        {
            var updatedCommentIds = updatedComments.Select(c => c.Id).ToHashSet();
            var commentsToRemove = existingBlog.Comments.Where(c => !updatedCommentIds.Contains(c.Id));
            context.Comments.RemoveRange(commentsToRemove);
            foreach (var updatedComment in updatedComments)
            {
                var existingComment = existingBlog.Comments.FirstOrDefault(c => c.Id == updatedComment.Id);
                if (existingComment is not null)
                {
                    context.Entry(existingComment).CurrentValues.SetValues(updatedComment);
                }
                else
                {
                    existingBlog.Comments.Add(updatedComment);
                }
            }
        }
    }
}
