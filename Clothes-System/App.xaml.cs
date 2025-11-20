using Application.Services;
using Clothes_System.ViewModels;
using Clothes_System.Views;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository_Interfaces;
using Infrastructure.Data.EF;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Clothes_System
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppContext.BaseDirectory);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }


        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

            // Register IConfiguration in the DI container
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDbContext<ClothesSystemDbContext>(options =>
            {
                var connString = configuration.GetConnectionString("DefaultConnection");

                // Expand environment variables (%APPDATA%)
                connString = Environment.ExpandEnvironmentVariables(connString);

                // Make sure folder exists
                var folder = System.IO.Path.GetDirectoryName(connString.Replace("Data Source=", "").Trim());
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                // Use the SQLite database
                options.UseSqlite(connString);
            });



            // Generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Specific repositories
            services.AddScoped<IWorkerRepository, WorkerRepository>();

            // Services
            services.AddScoped<ClinteService>();
            services.AddScoped<MaterialService>();
            services.AddScoped<BankService>();
            services.AddScoped<StorageService>();
            services.AddScoped<WorkerService>();
            services.AddScoped<ModelService>();
            services.AddScoped<ScissorService>();
            services.AddScoped<InvoiceService>();

            // ViewModels
            services.AddTransient<WorkerViewModel>();
            services.AddTransient<BankViewModel>();
            services.AddTransient<ModelViewModel>();
            services.AddTransient<ScissorViewModel>();
            services.AddTransient<ClinteViewModel>();

            // Views
            services.AddTransient<WorkerView>();
            services.AddTransient<ClinteView>();
            services.AddTransient<MaterialView>();
            services.AddTransient<BankView>();
            services.AddTransient<StorageView>();
            services.AddTransient<ModelView>();
            services.AddTransient<ScissorView>(); // ✅ optional

            // Main window
            services.AddSingleton<MainWindow>();
        }

        // ✅ Helper method added safely — lets you use App.GetService<T>() anywhere
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await SeedDatabaseAsync();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static async Task SeedDatabaseAsync()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClothesSystemDbContext>();

            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            var connString = configuration.GetConnectionString("DefaultConnection");

            // Extract the SQLite file path
            var dbFilePath = connString.Replace("Data Source=", "").Trim();
            dbFilePath = Environment.ExpandEnvironmentVariables(dbFilePath);

            // Ensure folder exists
            var folder = System.IO.Path.GetDirectoryName(dbFilePath);
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            Console.WriteLine("DB will be created at: " + dbFilePath);

            // Ensure DB is created
            await db.Database.EnsureCreatedAsync();

            // Force a dummy query to open the connection
            var _ = await db.Banks.CountAsync();

            // Seed default Bank
            if (!await db.Banks.AnyAsync())
            {
                db.Banks.Add(new Bank
                {
                    TotalAmount = 10000m
                });
            }

            await db.SaveChangesAsync();
        }



    }
}
