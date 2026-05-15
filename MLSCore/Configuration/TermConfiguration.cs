using MLSCore.Constants;
using MLSCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace MLSCore.Configuration
{
    public class TermConfiguration : IEntityTypeConfiguration<TbTerm>
    {
        public void Configure(EntityTypeBuilder<TbTerm> builder)
        {
            builder.Property(a => a.Name).IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.ImageName).HasDefaultValue(ProjConst.DefaulImage).HasMaxLength(100);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
           
        }
    }
}
