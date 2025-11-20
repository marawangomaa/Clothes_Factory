using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Data.EF
{
    public class ClothesSystemDbContext : DbContext
    {
        public ClothesSystemDbContext(DbContextOptions<ClothesSystemDbContext> options)
            : base(options) { }

        public DbSet<Clinte> Clintes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Scissor> Scissors { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<WorkerPiece> WorkerPieces { get; set; }

        public DbSet<Material> Materials { get; set; }
        public DbSet<Storage> Storages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClothesSystemDbContext).Assembly);

            modelBuilder.Entity<Bank>().HasData(new Bank
            {
                ID = 1,
                TotalAmount = 10000m
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
