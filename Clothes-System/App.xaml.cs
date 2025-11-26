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

        private System.Windows.Threading.DispatcherTimer _backupTimer;

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
            services.AddScoped<BackupService>();

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
            // Create error log immediately
            System.IO.File.WriteAllText("debug_log.txt", $"=== APPLICATION START === {DateTime.Now}\n");

            try
            {
                System.IO.File.AppendAllText("debug_log.txt", $"Base Directory: {AppContext.BaseDirectory}\n");
                System.IO.File.AppendAllText("debug_log.txt", $"Current Directory: {Environment.CurrentDirectory}\n");

                base.OnStartup(e);
                System.IO.File.AppendAllText("debug_log.txt", $"Base startup completed at {DateTime.Now}\n");

                // Create initial backup on startup
                await CreateInitialBackupAsync();
                System.IO.File.AppendAllText("debug_log.txt", $"Backup completed at {DateTime.Now}\n");

                // Seed database (this will now create tables)
                await SeedDatabaseAsync();
                System.IO.File.AppendAllText("debug_log.txt", $"Database seeded at {DateTime.Now}\n");

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                System.IO.File.AppendAllText("debug_log.txt", $"Main window obtained at {DateTime.Now}\n");

                mainWindow.Show();
                System.IO.File.AppendAllText("debug_log.txt", $"Main window shown at {DateTime.Now}\n");

                StartBackupScheduler();
                System.IO.File.AppendAllText("debug_log.txt", $"Backup scheduler started at {DateTime.Now}\n");

                System.IO.File.AppendAllText("debug_log.txt", $"=== APPLICATION STARTED SUCCESSFULLY === {DateTime.Now}\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("debug_log.txt", $"=== CRASH === {DateTime.Now}\n");
                System.IO.File.AppendAllText("debug_log.txt", $"Message: {ex.Message}\n");
                System.IO.File.AppendAllText("debug_log.txt", $"Type: {ex.GetType().FullName}\n");
                System.IO.File.AppendAllText("debug_log.txt", $"Stack Trace:\n{ex.StackTrace}\n");

                if (ex.InnerException != null)
                {
                    System.IO.File.AppendAllText("debug_log.txt", $"Inner Exception: {ex.InnerException.Message}\n");
                    System.IO.File.AppendAllText("debug_log.txt", $"Inner Stack Trace:\n{ex.InnerException.StackTrace}\n");
                }

                MessageBox.Show($"Application failed to start. Check debug_log.txt for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private async Task CreateInitialBackupAsync()
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();
                var result = await backupService.CreateBackupAsync();

                if (result.Success)
                    System.Diagnostics.Debug.WriteLine("Initial backup created successfully");
                else
                    System.Diagnostics.Debug.WriteLine($"Initial backup warning: {result.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initial backup failed: {ex.Message}");
            }
        }

        private static async Task SeedDatabaseAsync()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClothesSystemDbContext>();

            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            var connString = configuration.GetConnectionString("DefaultConnection");

            // Extract the SQLite file path
            connString = Environment.ExpandEnvironmentVariables(connString);

            // Ensure folder exists - EXTRACT FILE PATH FIRST
            var dbFilePath = connString.Replace("Data Source=", "").Trim();
            var folder = System.IO.Path.GetDirectoryName(dbFilePath);
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            Console.WriteLine("DB will be created at: " + dbFilePath);

            try
            {
                // ✅ FIX: Apply database migrations first
                System.IO.File.AppendAllText("debug_log.txt", "Applying database migrations...\n");
                await db.Database.MigrateAsync();
                System.IO.File.AppendAllText("debug_log.txt", "Database migrations applied successfully\n");

                // ✅ Now check if we have any banks
                System.IO.File.AppendAllText("debug_log.txt", "Checking if database needs seeding...\n");
                var bankCount = await db.Banks.CountAsync();
                System.IO.File.AppendAllText("debug_log.txt", $"Found {bankCount} banks in database\n");

                // Seed default Bank only if empty
                if (bankCount == 0)
                {
                    System.IO.File.AppendAllText("debug_log.txt", "Seeding default bank...\n");
                    db.Banks.Add(new Bank
                    {
                        TotalAmount = 130000m
                    });
                    await db.SaveChangesAsync();
                    System.IO.File.AppendAllText("debug_log.txt", "Default bank seeded successfully\n");
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("debug_log.txt", $"Database error: {ex.Message}\n");
                System.IO.File.AppendAllText("debug_log.txt", $"Stack trace: {ex.StackTrace}\n");
                throw;
            }
        }

        private void StartBackupScheduler()
        {
            _backupTimer = new System.Windows.Threading.DispatcherTimer();
            _backupTimer.Interval = TimeSpan.FromHours(24); // Run every 24 hours
            _backupTimer.Tick += async (s, e) => await CreateDailyBackupAsync();
            _backupTimer.Start();
        }

        private async Task CreateDailyBackupAsync()
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();
                var result = await backupService.CreateBackupAsync();

                System.Diagnostics.Debug.WriteLine($"Daily backup: {result.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Daily backup failed: {ex.Message}");
            }
        }
    }
}