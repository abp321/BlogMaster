using BlogMaster.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogMaster.Database
{
    public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
    {
        public DbSet<BlogEntity> Blogs { get; set; }
        public DbSet<CommentEntity> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
