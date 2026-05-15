using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<TbAssignment>
    {
        public void Configure(EntityTypeBuilder<TbAssignment> builder)
        {
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.TotalMarks).HasDefaultValue(100.0);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

            builder.HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
