using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using HMZ.Database.Entities;

namespace HMZ.Database.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // default
            builder.HasKey(x => x.Id);
            #region Custom 
            builder.Property(x => x.Key).HasMaxLength(40);
            builder.Property(x => x.Value).HasMaxLength(40);
            builder.Property(x => x.Description).HasMaxLength(150);
            #endregion Custom
        }
    }
}
