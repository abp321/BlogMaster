using BlogMaster.Database;
using BlogMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BlogMaster.Models.Entities;

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
                .Select(b => new BlogEntity
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = string.Empty,
                    ViewCount = b.ViewCount,
                    Author = b.Author,
                    PublishedDate = b.PublishedDate,
                    Comments = b.Comments.Select(c => new CommentEntity
                    {
                        Id = c.Id,
                        Author = c.Author,
                        Content = string.Empty,
                        PostedDate = c.PostedDate,
                        BlogPostId = c.BlogPostId
                    }).ToList()

                })
                .ToListAsync();
        }

        public async Task<BlogEntity?> CreateBlog(BlogEntity blogEntity)
        {
            try
            {
                blogEntity.PublishedDate = DateTime.UtcNow;
                context.Blogs.Add(blogEntity);
                await context.SaveChangesAsync();
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
                var existingBlog = await context.Blogs
                    .Include(b => b.Comments)
                    .FirstOrDefaultAsync(b => b.Id == blogEntity.Id);

                if (existingBlog is null) return null;

                context.Entry(existingBlog).CurrentValues.SetValues(blogEntity);
                UpdateComments(existingBlog, blogEntity.Comments);

                await context.SaveChangesAsync();
                return await context.Blogs
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
                var blog = await context.Blogs.FindAsync(id);

                if (blog is null) return false;

                context.Blogs.Remove(blog);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
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
