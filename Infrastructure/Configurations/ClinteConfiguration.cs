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
    public class ClinteConfiguration : IEntityTypeConfiguration<Clinte>
    {
        public void Configure(EntityTypeBuilder<Clinte> builder)
        {
            builder.HasKey(c => c.ID);
            builder.Property(c => c.Name).HasMaxLength(200);
            builder.Property(c => c.Ph_Number).HasMaxLength(50);
            builder.Property(c => c.Location).HasMaxLength(300);

            builder.HasMany(c => c.Invoices)
                   .WithOne(i => i.Clinte)
                   .HasForeignKey(i => i.ClinteID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
