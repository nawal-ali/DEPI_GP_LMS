using MLSCore.Constants;
using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    /// <summary>
    /// Entity Framework Core configuration for TbParent
    /// Defines constraints, defaults, and relationships
    /// </summary>
    public class ParentConfiguration : IEntityTypeConfiguration<TbParent>
    {
        public void Configure(EntityTypeBuilder<TbParent> builder)
        {
            // Key configuration
            builder.HasKey(p => p.Id);

            // Required fields
            builder.Property(p => p.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.UserId)
                .IsRequired()
                .HasMaxLength(256);

            // Optional fields with max length
            builder.Property(p => p.Relationship)
                .HasMaxLength(50);

            builder.Property(p => p.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(p => p.AlternativePhoneNumber)
                .HasMaxLength(20);

            builder.Property(p => p.Email)
                .HasMaxLength(256);

            builder.Property(p => p.NationalId)
                .HasMaxLength(50);

            builder.Property(p => p.ImageName)
                .HasMaxLength(255)
                .HasDefaultValue(ProjConst.UserImage);

            builder.Property(p => p.Address)
                .HasMaxLength(500);

            builder.Property(p => p.City)
                .HasMaxLength(100);

            builder.Property(p => p.PostalCode)
                .HasMaxLength(20);

            builder.Property(p => p.Occupation)
                .HasMaxLength(100);

            // Default values
            builder.Property(p => p.CurrentState)
                .HasDefaultValue(1);

            builder.Property(p => p.IsPrimaryGuardian)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Audit fields max length
            builder.Property(p => p.CreatedBy)
                .HasMaxLength(256);

            builder.Property(p => p.UpdatedBy)
                .HasMaxLength(256);

            // Foreign key configuration
            builder.HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<TbParent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Navigation property configuration
            builder.HasMany(p => p.Children)
                .WithOne(s => s.Parent)
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index for efficient queries
            builder.HasIndex(p => p.UserId).IsUnique();
            builder.HasIndex(p => p.NationalId);
            builder.HasIndex(p => p.PhoneNumber);
            builder.HasIndex(p => p.CurrentState);
        }
    }
}
