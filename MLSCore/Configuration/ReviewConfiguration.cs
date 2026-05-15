using MLSCore.Constants;
using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace MLSCore.Configuration
{
    public class ReviewConfiguration : IEntityTypeConfiguration<TbCourseReview>
    {
        public void Configure(EntityTypeBuilder<TbCourseReview> builder)
        {
            builder.Property(a => a.Review).IsRequired()
                .HasMaxLength(200);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(a => a.Rating).HasDefaultValue(5);


        }
    }
}