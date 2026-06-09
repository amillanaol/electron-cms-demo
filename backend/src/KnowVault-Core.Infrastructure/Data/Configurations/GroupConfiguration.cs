using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder.Property(g => g.Slug)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.HasIndex(g => g.Slug)
            .IsUnique();

        builder.Property(g => g.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(g => g.Permissions)
            .WithOne(p => p.Group)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
