using BlogMaster.Database;
using Microsoft.EntityFrameworkCore;

namespace BlogMaster
{
    public static class Scripts
    {
        public static async Task<string> InitializeSQLiteDatabase()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFolderPath = Path.Combine(localAppData, "BlogMaster");
            string dbFilePath = Path.Combine(dbFolderPath, "BlogData.db");

            Directory.CreateDirectory(dbFolderPath);
            string connectionString = $"Data Source={dbFilePath}";

            DbContextOptionsBuilder<BlogDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlite(connectionString);
            await using BlogDbContext context = new(optionsBuilder.Options);
            await context.Database.EnsureCreatedAsync();

            return connectionString;
        }
    }
}
