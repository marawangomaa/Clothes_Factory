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
    public class WorkerPieceDetailConfiguration : IEntityTypeConfiguration<WorkerPieceDetail>
    {
        public void Configure(EntityTypeBuilder<WorkerPieceDetail> builder)
        {
            builder.HasKey(wp => wp.ID);

            builder.Property(wp => wp.Pieces)
                   .IsRequired();

            builder.Property(wp => wp.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(wp => wp.Date)
                   .IsRequired();

            // Relationship with Worker
            builder.HasOne(wp => wp.Worker)
                   .WithMany(w => w.DailyPieces)
                   .HasForeignKey(wp => wp.WorkerID)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship with Model
            builder.HasOne(wp => wp.Model)
                   .WithMany()
                   .HasForeignKey(wp => wp.ModelID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
