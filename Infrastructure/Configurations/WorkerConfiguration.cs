using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
    {
        public void Configure(EntityTypeBuilder<Worker> builder)
        {
            builder.HasKey(w => w.ID);

            builder.Property(w => w.Name).HasMaxLength(150);
            builder.Property(w => w.Price_Count).HasColumnType("decimal(18,2)");
            // BankTransactions navigation configured in BankTransactionConfiguration
        }
    }
}
