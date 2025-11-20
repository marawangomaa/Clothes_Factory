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
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(m => m.ID);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Price).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(m => m.Quantity).IsRequired();
            builder.Property(m => m.Date).IsRequired();

            builder.HasOne(m => m.Bank)
                   .WithMany()
                   .HasForeignKey(m => m.BankID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
