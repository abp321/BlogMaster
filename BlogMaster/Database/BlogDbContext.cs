using BlogMaster.Client.Utility;
using BlogMaster.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogMaster.Database
{
    public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
    {
        public DbSet<BlogEntity> Blogs { get; set; }
        public DbSet<CommentEntity> Comments { get; set; }
        public DbSet<VisitorEntity> Visitors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentEntity>()
                .HasOne(c => c.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            return BackgroundTask.Factory.StartNew(base.SaveChanges).Result;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => base.SaveChangesAsync(cancellationToken), cancellationToken).Unwrap();
        }
    }
}
