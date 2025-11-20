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
    public class StorageConfiguration : IEntityTypeConfiguration<Storage>
    {
        public void Configure(EntityTypeBuilder<Storage> builder)
        {
            builder.HasKey(s => s.ID);

            builder.Property(s => s.Product_Name).HasMaxLength(200);
            builder.Property(s => s.Product_Type).HasMaxLength(100);

            //builder.HasMany(s => s.Materials)
            //       .WithOne(m => m.Storage)
            //       .HasForeignKey(m => m.StorageID)
            //       .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Models)
                   .WithOne(m => m.Storage)
                   .HasForeignKey(m => m.StorageID)
                   .OnDelete(DeleteBehavior.Restrict);

            // InvoiceStorages mapping left as-is (many-to-many-like)
        }
    }
}
