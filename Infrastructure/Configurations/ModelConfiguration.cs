using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.HasKey(m => m.ID);

            builder.Property(m => m.Code).HasMaxLength(100);
            builder.Property(m => m.Cost).HasColumnType("decimal(18,2)");
            builder.Property(m => m.SellPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.Quantity).IsRequired();

            builder.HasOne(m => m.Storage)
                   .WithMany(s => s.Models)
                   .HasForeignKey(m => m.StorageID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
