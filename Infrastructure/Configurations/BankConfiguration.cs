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
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.HasKey(b => b.ID);

            builder.Property(b => b.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.HasMany(b => b.Transactions)
                   .WithOne(t => t.Bank)
                   .HasForeignKey(t => t.BankID)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
