using BlogMaster.Database;
using BlogMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BlogMaster.Models.Entities;
using BlogMaster.Client.Utility;

namespace BlogMaster.Services.Implementations
{
    public class BlogSqlService(BlogDbContext context) : IBlogSqlService
    {
        public async Task<BlogEntity?> GetBlog(int id)
        {
            return await context.Blogs
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BlogEntity>> GetBlogs()
        {
            return await context.Blogs
                .Include(b => b.Comments)
                .ToListAsync();
        }

        public Task<BlogEntity?> CreateBlog(BlogEntity blogEntity)
        {
            return BackgroundTask.Run(() => 
            {
                try
                {
                    blogEntity.PublishedDate = DateTime.UtcNow;
                    context.Blogs.Add(blogEntity);
                    context.SaveChanges();
                    return blogEntity;
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task<BlogEntity?> UpdateBlog(BlogEntity blogEntity)
        {
            return BackgroundTask.Run(() => 
            {
                try
                {
                    var existingBlog = context.Blogs
                        .Include(b => b.Comments)
                        .FirstOrDefault(b => b.Id == blogEntity.Id);

                    if (existingBlog is null) return null;

                    context.Entry(existingBlog).CurrentValues.SetValues(blogEntity);
                    UpdateComments(existingBlog, blogEntity.Comments);

                    context.SaveChanges();
                    return context.Blogs
                        .Include(b => b.Comments)
                        .FirstOrDefault(b => b.Id == existingBlog.Id);
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task<bool> DeleteBlog(int id)
        {
            return BackgroundTask.Run(() => 
            {
                try
                {
                    var blog = context.Blogs.Find(id);

                    if (blog is null) return false;

                    context.Blogs.Remove(blog);
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public IAsyncEnumerable<VisitorEntity> GetVisitors()
        {
            return context.Visitors.AsAsyncEnumerable();
        }

        private void UpdateComments(BlogEntity existingBlog, List<CommentEntity> updatedComments)
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
