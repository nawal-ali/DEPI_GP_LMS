using MLSCore.Constants;
using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    public class InstructorConfiguration : IEntityTypeConfiguration<TbInstructor>
    {
        public void Configure(EntityTypeBuilder<TbInstructor> builder)
        {
            builder.Property(a => a.FullName).IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.ImageName).HasDefaultValue(ProjConst.UserImage);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

        }
    }
}
