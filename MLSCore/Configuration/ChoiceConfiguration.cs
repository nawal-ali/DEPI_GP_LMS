using MLSCore.Constants;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Configuration
{
    public class ChoiceConfiguration : IEntityTypeConfiguration<TbChoice>
    {
        public void Configure(EntityTypeBuilder<TbChoice> builder)
        {
            builder.Property(a => a.Title).IsRequired()
                .HasMaxLength(100);
           
            builder.Property(a => a.Correct).HasDefaultValue(false);
           

        }
    }
}
