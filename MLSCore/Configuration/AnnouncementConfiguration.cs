using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    /// <summary>
    /// Entity Framework Core configuration for TbAnnouncement
    /// Defines constraints, defaults, and relationships for the announcement system
    /// </summary>
    public class AnnouncementConfiguration : IEntityTypeConfiguration<TbAnnouncement>
    {
        public void Configure(EntityTypeBuilder<TbAnnouncement> builder)
        {
            // Key configuration
            builder.HasKey(a => a.Id);

            // Required fields
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Content)
                .IsRequired();

            // Optional fields with max length
            builder.Property(a => a.Description)
                .HasMaxLength(500);

            builder.Property(a => a.TargetAudience)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("All");

            builder.Property(a => a.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            builder.Property(a => a.Category)
                .HasMaxLength(50);

            builder.Property(a => a.ImageUrl)
                .HasMaxLength(500);

            builder.Property(a => a.AttachmentUrl)
                .HasMaxLength(500);

            builder.Property(a => a.CreatedBy)
                .HasMaxLength(256);

            builder.Property(a => a.UpdatedBy)
                .HasMaxLength(256);

            builder.Property(a => a.CreatedByUserId)
                .HasMaxLength(256);

            // Default values
            builder.Property(a => a.PublishedDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.Property(a => a.IsPinned)
                .HasDefaultValue(false);

            builder.Property(a => a.RequiresAcknowledgment)
                .HasDefaultValue(false);

            builder.Property(a => a.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(a => a.CurrentState)
                .HasDefaultValue(1);

            builder.Property(a => a.ViewCount)
                .HasDefaultValue(0);

            builder.Property(a => a.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Foreign key relationships
            builder.HasOne(a => a.Creator)
                .WithMany(u => u.CreatedAnnouncements)
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Term)
                .WithMany()
                .HasForeignKey(a => a.TermId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(a => a.Grade)
                .WithMany()
                .HasForeignKey(a => a.GradeId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for efficient queries
            builder.HasIndex(a => a.PublishedDate);
            builder.HasIndex(a => a.IsActive);
            builder.HasIndex(a => a.IsPinned);
            builder.HasIndex(a => a.CurrentState);
            builder.HasIndex(a => a.TargetAudience);
            builder.HasIndex(a => a.CreatedByUserId);
            builder.HasIndex(a => a.CourseId);
            builder.HasIndex(a => a.TermId);

            // Composite index for common queries
            builder.HasIndex(a => new { a.IsActive, a.CurrentState, a.PublishedDate })
                .HasName("IX_Announcement_ActiveCurrent");

            builder.HasIndex(a => new { a.TargetAudience, a.IsActive, a.PublishedDate })
                .HasName("IX_Announcement_AudienceActive");
        }
    }
}
