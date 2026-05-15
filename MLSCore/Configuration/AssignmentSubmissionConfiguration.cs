using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<TbAssignmentSubmission>
    {
        public void Configure(EntityTypeBuilder<TbAssignmentSubmission> builder)
        {
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

            builder.HasOne(a => a.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(a => a.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
