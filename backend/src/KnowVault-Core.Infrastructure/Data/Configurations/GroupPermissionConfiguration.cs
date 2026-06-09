using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class GroupPermissionConfiguration : IEntityTypeConfiguration<GroupPermission>
{
    public void Configure(EntityTypeBuilder<GroupPermission> builder)
    {
        builder.ToTable("group_permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Resource)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.Property(p => p.Action)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.HasIndex(p => new { p.GroupId, p.Resource, p.Action })
            .IsUnique();
    }
}
