using MLSCore.Constants;
using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MLSCore.Configuration
{
    public class GradeConfiguration : IEntityTypeConfiguration<TbGrade>
    {
        public void Configure(EntityTypeBuilder<TbGrade> builder)
        {
            builder.Property(a => a.Name).IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.ImageName).HasDefaultValue(ProjConst.DefaulImage).HasMaxLength(100);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
            //builder.HasData(new TbGrade
            //{
            //    Name = "Primary Grade 1",
            //    StageId=1,
                

            //});

        }
    }
}
