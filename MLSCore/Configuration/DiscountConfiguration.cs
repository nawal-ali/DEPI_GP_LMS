using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Configuration
{
    public class DiscountConfiguration : IEntityTypeConfiguration<TbCourseDiscount>
    {
        public void Configure(EntityTypeBuilder<TbCourseDiscount> builder)
        {
           
            builder.Property(a => a.CurrentState).HasDefaultValue(1);
          
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");

        }
    }
}

