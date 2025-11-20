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
    public class InvoiceModelConfiguration : IEntityTypeConfiguration<InvoiceModel>
    {
        public void Configure(EntityTypeBuilder<InvoiceModel> builder)
        {
            builder.HasKey(im => im.ID);

            builder.Property(im => im.Quantity).IsRequired();
            builder.Property(im => im.UnitPrice).HasColumnType("decimal(18,2)");

            builder.HasOne(im => im.Invoice)
                   .WithMany(i => i.InvoiceModels)
                   .HasForeignKey(im => im.InvoiceID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(im => im.Model)
                   .WithMany(m => m.InvoiceModels)
                   .HasForeignKey(im => im.ModelID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
