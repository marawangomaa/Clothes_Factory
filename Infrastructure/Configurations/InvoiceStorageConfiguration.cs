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
    public class InvoiceStorageConfiguration : IEntityTypeConfiguration<InvoiceStorage>
    {
        public void Configure(EntityTypeBuilder<InvoiceStorage> builder)
        {
            builder.HasKey(isr => new { isr.InvoiceID, isr.StorageID });

            builder.HasOne(isr => isr.Invoice)
                   .WithMany(i => i.InvoiceStorages)
                   .HasForeignKey(isr => isr.InvoiceID);

            builder.HasOne(isr => isr.Storage)
                   .WithMany(s => s.InvoiceStorages)
                   .HasForeignKey(isr => isr.StorageID);
        }
    }
}
