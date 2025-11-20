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
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.ID);

            builder.Property(i => i.Date).IsRequired();
            builder.Property(i => i.Number).HasMaxLength(50);
            builder.Property(i => i.PaymentMethod).HasMaxLength(50);
            builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");

            builder.HasOne(i => i.Clinte)
                   .WithMany(c => c.Invoices)
                   .HasForeignKey(i => i.ClinteID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Bank)
                   .WithMany() // bank->invoices not necessary; transactions will be used for ledger
                   .HasForeignKey(i => i.BankID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.InvoiceModels)
                   .WithOne(im => im.Invoice)
                   .HasForeignKey(im => im.InvoiceID)
                   .OnDelete(DeleteBehavior.Cascade);

            // Optional: if you kept Invoice.Transactions collection, configure it:
            builder.HasMany(i => i.Transactions)
                   .WithOne(t => t.Invoice)
                   .HasForeignKey(t => t.InvoiceID)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
