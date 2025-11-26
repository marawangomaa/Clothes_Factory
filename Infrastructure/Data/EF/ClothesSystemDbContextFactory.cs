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

            var connString = configuration.GetConnectionString("DefaultConnection");
            connString = Environment.ExpandEnvironmentVariables(connString);

            // Ensure folder exists - EXTRACT FILE PATH FIRST
            var dbFilePath = connString.Replace("Data Source=", "").Trim();
            var folder = System.IO.Path.GetDirectoryName(dbFilePath);
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var optionsBuilder = new DbContextOptionsBuilder<ClothesSystemDbContext>();
            optionsBuilder.UseSqlite(connString);

            return new ClothesSystemDbContext(optionsBuilder.Options);
        }
    }

}
