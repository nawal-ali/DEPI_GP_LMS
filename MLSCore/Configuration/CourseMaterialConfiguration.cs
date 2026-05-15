using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    public class CourseMaterialConfiguration : IEntityTypeConfiguration<TbCourseMaterial>
    {
        public void Configure(EntityTypeBuilder<TbCourseMaterial> builder)
        {
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(200);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

            builder.HasOne(a => a.Course)
                .WithMany(c => c.CourseMaterials)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
