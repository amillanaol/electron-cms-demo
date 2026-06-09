using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class ContentDocumentTagConfiguration : IEntityTypeConfiguration<ContentDocumentTag>
{
    public void Configure(EntityTypeBuilder<ContentDocumentTag> builder)
    {
        builder.ToTable("content_document_tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.HasIndex(t => new { t.DocumentId, t.Name }).IsUnique();
    }
}

