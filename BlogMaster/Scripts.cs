using BlogMaster.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlogMaster
{
    public static class Scripts
    {
        public static async Task<string> InitializeSQLiteDatabase()
        {
            string homePath = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFolderPath = Path.Combine(homePath, "BlogMaster");
            string dbFilePath = Path.Combine(dbFolderPath, "BlogData.db");
            Directory.CreateDirectory(dbFolderPath);

            string connectionString = $"Data Source={dbFilePath}";
            DbContextOptionsBuilder<BlogDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlite(connectionString);
            await using BlogDbContext context = new(optionsBuilder.Options);
            await context.Database.EnsureCreatedAsync();
            _ = InitializeTableIfMissing(context);

            return connectionString;
        }

        public static void LogVisitor(string ipAddress, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            const string sql = @"
        INSERT INTO Visitors (IpAddress, LastVisit, VisitCount)
        VALUES (@ipAddress, @lastVisit, 1)
        ON CONFLICT(IpAddress) DO UPDATE SET
            LastVisit = @lastVisit,
            VisitCount = VisitCount + 1;";

            SqliteParameter[] parameters = [ 
                new SqliteParameter("@ipAddress", ipAddress),
                new SqliteParameter("@lastVisit", DateTime.UtcNow) 
                ];

            context.Database.ExecuteSqlRaw(sql, parameters);
        }

        private static async Task InitializeTableIfMissing(BlogDbContext context)
        {
            var tableName = "Visitors";
            var tableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";

            var tableExists = await context.Database.ExecuteSqlRawAsync(tableExistsQuery) > 0;

            if (!tableExists)
            {
                var createTableQuery = @"
            CREATE TABLE Visitors (
                IpAddress TEXT PRIMARY KEY NOT NULL,
                LastVisit DATETIME NOT NULL,
                VisitCount INTEGER NOT NULL
            );";

                await context.Database.ExecuteSqlRawAsync(createTableQuery);
            }
        }
    }
}
