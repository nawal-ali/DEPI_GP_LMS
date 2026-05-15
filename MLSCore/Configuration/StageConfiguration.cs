
using MLSCore.Constants;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.Configuration
{
    public class  StageConfiguration : IEntityTypeConfiguration<TbStage>
    {
        public void Configure(EntityTypeBuilder<TbStage> builder)
    {
        builder.Property(a => a.Name).IsRequired()
            .HasMaxLength(100);
        builder.Property(a => a.CurrentState).HasDefaultValue(1);
        builder.Property(a => a.ImageName).HasDefaultValue(ProjConst.DefaulImage).HasMaxLength(100);
        builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
        
    }
}
}
