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
    public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
    {
        public void Configure(EntityTypeBuilder<BankTransaction> builder)
        {
            builder.HasKey(bt => bt.ID);

            builder.Property(bt => bt.Date)
                   .IsRequired();

            builder.Property(bt => bt.Type)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(bt => bt.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(bt => bt.TotalAfterTransaction)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // Optional relationships to Invoice / Material / Worker
            builder.HasOne(bt => bt.Invoice)
                   .WithMany(i => i.Transactions)
                   .HasForeignKey(bt => bt.InvoiceID)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(bt => bt.Material)
                   .WithMany() // not mapping material->transactions collection
                   .HasForeignKey(bt => bt.MaterialID)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(bt => bt.Worker)
                   .WithMany(w => w.BankTransactions)
                   .HasForeignKey(bt => bt.WorkerID)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
