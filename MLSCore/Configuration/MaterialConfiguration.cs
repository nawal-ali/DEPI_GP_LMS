using MLSCore.Constants;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Configuration
{
    public class MaterialConfiguration : IEntityTypeConfiguration<TbMterials>
    {
        public void Configure(EntityTypeBuilder<TbMterials> builder)
        {
            builder.Property(a => a.Title).IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.Description).HasMaxLength(200);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
         
        }
    }
}
