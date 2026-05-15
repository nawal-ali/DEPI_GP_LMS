using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Configuration
{
    public class SbContentConfiguration : IEntityTypeConfiguration<TbSubContent>
    {
        public void Configure(EntityTypeBuilder<TbSubContent> builder)
        {
            builder.Property(a => a.Title).IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
            builder.Property(a => a.Description).HasMaxLength(200);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

        }
    }
}
