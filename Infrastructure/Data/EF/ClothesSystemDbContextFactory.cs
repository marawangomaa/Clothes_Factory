using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.EF
{
    public class ClothesSystemDbContextFactory : IDesignTimeDbContextFactory<ClothesSystemDbContext>
    {
        public ClothesSystemDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            // Get SQLite file path
            var connString = configuration.GetConnectionString("DefaultConnection");
            var dbFilePath = connString.Replace("Data Source=", "").Trim();
            dbFilePath = Environment.ExpandEnvironmentVariables(dbFilePath);

            // Ensure folder exists
            var folder = System.IO.Path.GetDirectoryName(dbFilePath);
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var optionsBuilder = new DbContextOptionsBuilder<ClothesSystemDbContext>();
            optionsBuilder.UseSqlite(dbFilePath);

            return new ClothesSystemDbContext(optionsBuilder.Options);
        }

    }

}
