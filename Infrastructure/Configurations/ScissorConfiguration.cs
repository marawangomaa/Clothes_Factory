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
    public class ScissorConfiguration : IEntityTypeConfiguration<Scissor>
    {
        public void Configure(EntityTypeBuilder<Scissor> builder)
        {
            builder.HasKey(s => s.ID);
            builder.Property(s => s.Model)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.Number)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.Date)
                   .IsRequired();
        }
    }
}
