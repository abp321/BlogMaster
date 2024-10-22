using BlogMaster.Models.Entities;
using BlogMaster.Services.Interfaces;
using BlogMaster.Shared.Interfaces;
using BlogMaster.Shared.Models;

namespace BlogMaster.Services.Implementations
{
    public class BlogService(IBlogSqlService blogSqlService) : IBlogService
    {
        public async Task<BlogDto?> GetBlog(int id)
        {
            var blogEntity = await blogSqlService.GetBlog(id);

            if (blogEntity is null) return null;
            var blogDto = MapBlogEntityToDto(blogEntity);
            return blogDto;
        }

        public async Task<List<BlogDto>?> GetBlogs()
        {
            var blogEntities = await blogSqlService.GetBlogs();
            var blogDtos = blogEntities.Select(MapBlogEntityToDto).ToList();
            return blogDtos;
        }

        public async Task<BlogDto?> CreateBlog(BlogDto dto)
        {
            var blogEntity = MapBlogDtoToEntity(dto);
            var createdEntity = await blogSqlService.CreateBlog(blogEntity);

            if (createdEntity is not null)
            {
               return MapBlogEntityToDto(createdEntity);
            }
            else
            {
                return null;
            }
        }

        public async Task<BlogDto?> UpdateBlog(BlogDto dto)
        {
            var blogEntity = MapBlogDtoToEntity(dto);
            var updatedEntity = await blogSqlService.UpdateBlog(blogEntity);

            if (updatedEntity is not null)
            {
                return MapBlogEntityToDto(updatedEntity);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteBlog(int id)
        {
            var result = await blogSqlService.DeleteBlog(id);
            return result;
        }

        private static BlogDto MapBlogEntityToDto(BlogEntity blog)
        {
            return new()
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Author = blog.Author,
                PublishedDate = blog.PublishedDate,
                ViewCount = blog.ViewCount,
                Comments = blog.Comments.Select(MapCommentEntityToDto).ToList()
            };
        }

        private static CommentDto MapCommentEntityToDto(CommentEntity comment)
        {
            return new()
            {
                Id = comment.Id,
                Author = comment.Author,
                Content = comment.Content,
                PostedDate = comment.PostedDate,
                BlogPostId = comment.BlogPostId
            };
        }

        private static BlogEntity MapBlogDtoToEntity(BlogDto dto)
        {
            return new()
            {
                Id = dto.Id,
                Title = dto.Title,
                Content = dto.Content,
                Author = dto.Author,
                PublishedDate = dto.PublishedDate,
                ViewCount = dto.ViewCount,
                Comments = dto.Comments.Select(MapCommentDtoToEntity).ToList()
            };
        }

        private static CommentEntity MapCommentDtoToEntity(CommentDto dto)
        {
            return new()
            {
                Id = dto.Id,
                Author = dto.Author,
                Content = dto.Content,
                PostedDate = dto.PostedDate,
                BlogPostId = dto.BlogPostId
            };
        }
    }
}
