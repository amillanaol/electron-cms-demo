using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class ContentDocumentVersionConfiguration : IEntityTypeConfiguration<ContentDocumentVersion>
{
    public void Configure(EntityTypeBuilder<ContentDocumentVersion> builder)
    {
        builder.ToTable("content_document_versions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Title)
            .HasColumnType("varchar(512)")
            .IsRequired();

        builder.Property(v => v.MarkdownBody)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(v => v.RenderedHtml)
            .HasColumnType("text");

        builder.Property(v => v.ChangeSummary)
            .HasColumnType("varchar(512)");

        builder.Property(v => v.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(v => v.CreatedBy)
            .HasColumnType("varchar(256)");

        builder.Property(v => v.IsCurrent)
            .HasDefaultValue(false);

        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();
        builder.HasIndex(v => new { v.DocumentId, v.IsCurrent });
    }
}

