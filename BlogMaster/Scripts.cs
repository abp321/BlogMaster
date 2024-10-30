using BlogMaster.Client.Utility;
using BlogMaster.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlogMaster
{
    public static class Scripts
    {
        public static Task<string> InitializeSQLiteDatabase()
        {
            return BackgroundTask.Run(() => 
            {
                string homePath = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dbFolderPath = Path.Combine(homePath, "BlogMaster");
                string dbFilePath = Path.Combine(dbFolderPath, "BlogData.db");
                Directory.CreateDirectory(dbFolderPath);

                string connectionString = $"Data Source={dbFilePath}";
                DbContextOptionsBuilder<BlogDbContext> optionsBuilder = new();
                optionsBuilder.UseSqlite(connectionString);
                using BlogDbContext context = new(optionsBuilder.Options);
                context.Database.EnsureCreated();
                InitializeTableIfMissing(context);

                return connectionString;
            });
        }

        public static void LogVisitor(string ipAddress, IServiceProvider serviceProvider)
        {
            BackgroundTask.Schedule(() => 
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
            });
        }


        private static void InitializeTableIfMissing(BlogDbContext context)
        {
            const string tableExistsQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Visitors';";

            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = tableExistsQuery;
            context.Database.OpenConnection();

            int tableCount = Convert.ToInt32(command.ExecuteScalar());

            if (tableCount == 0)
            {
                const string createTableQuery = @"
        CREATE TABLE Visitors (
            IpAddress TEXT PRIMARY KEY NOT NULL,
            LastVisit DATETIME NOT NULL,
            VisitCount INTEGER NOT NULL
        );";

                context.Database.ExecuteSqlRaw(createTableQuery);
            }
        }

    }
}
