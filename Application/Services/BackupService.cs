using Infrastructure.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ClothesSystemDbContext _dbContext;

        public BackupService(IConfiguration configuration, ClothesSystemDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public string BackupFolderPath
        {
            get
            {
                var appFolder = AppContext.BaseDirectory;
                var parentFolder = Directory.GetParent(appFolder)?.Parent?.FullName;
                return Path.Combine(parentFolder ?? appFolder, "BackUp");
            }
        }

        public async Task<(bool Success, string Message)> CreateBackupAsync()
        {
            try
            {
                // Ensure backup directory exists
                if (!Directory.Exists(BackupFolderPath))
                    Directory.CreateDirectory(BackupFolderPath);

                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var backupFolder = Path.Combine(BackupFolderPath, today);

                // Create today's backup folder
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                // Get database file path
                var connString = _configuration.GetConnectionString("DefaultConnection");
                connString = Environment.ExpandEnvironmentVariables(connString);
                var dbFilePath = connString.Replace("Data Source=", "").Trim();

                if (!File.Exists(dbFilePath))
                    return (false, "Database file not found");

                // Copy database file to backup folder
                var backupDbPath = Path.Combine(backupFolder, Path.GetFileName(dbFilePath));
                File.Copy(dbFilePath, backupDbPath, true);

                // Create backup info file
                var backupInfo = new
                {
                    BackupDate = DateTime.Now,
                    DatabaseSize = new FileInfo(dbFilePath).Length,
                    Version = "1.0.0"
                };

                var infoPath = Path.Combine(backupFolder, "backup-info.json");
                await File.WriteAllTextAsync(infoPath,
                    System.Text.Json.JsonSerializer.Serialize(backupInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                // Clean up old backups (keep last 30 days)
                await CleanupOldBackupsAsync();

                return (true, $"Backup created successfully at: {backupFolder}");
            }
            catch (Exception ex)
            {
                return (false, $"Backup failed: {ex.Message}");
            }
        }

        private async Task CleanupOldBackupsAsync()
        {
            try
            {
                if (!Directory.Exists(BackupFolderPath))
                    return;

                var backupFolders = Directory.GetDirectories(BackupFolderPath)
                    .Select(f => new { Path = f, Name = Path.GetFileName(f) })
                    .Where(f => DateTime.TryParse(f.Name, out _))
                    .Select(f => new { f.Path, Date = DateTime.Parse(f.Name) })
                    .OrderByDescending(f => f.Date)
                    .ToList();

                // Keep only last 30 days
                var oldBackups = backupFolders.Skip(30);
                foreach (var oldBackup in oldBackups)
                {
                    Directory.Delete(oldBackup.Path, true);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the backup process
                System.Diagnostics.Debug.WriteLine($"Cleanup warning: {ex.Message}");
            }
        }

        public string[] GetAvailableBackups()
        {
            if (!Directory.Exists(BackupFolderPath))
                return Array.Empty<string>();

            return Directory.GetDirectories(BackupFolderPath)
                .Select(Path.GetFileName)
                .OrderDescending()
                .ToArray();
        }

        public async Task<(bool Success, string Message)> RestoreBackupAsync(string backupDate)
        {
            try
            {
                var backupFolder = Path.Combine(BackupFolderPath, backupDate);
                if (!Directory.Exists(backupFolder))
                    return (false, "Backup not found");

                var connString = _configuration.GetConnectionString("DefaultConnection");
                connString = Environment.ExpandEnvironmentVariables(connString);
                var dbFilePath = connString.Replace("Data Source=", "").Trim();

                var backupDbPath = Path.Combine(backupFolder, Path.GetFileName(dbFilePath));
                if (!File.Exists(backupDbPath))
                    return (false, "Database file not found in backup");

                // Close database connections
                await _dbContext.Database.CloseConnectionAsync();

                // Replace current database with backup
                File.Copy(backupDbPath, dbFilePath, true);

                return (true, "Backup restored successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Restore failed: {ex.Message}");
            }
        }
    }
}
