using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class ContentDocumentConfiguration : IEntityTypeConfiguration<ContentDocument>
{
    public void Configure(EntityTypeBuilder<ContentDocument> builder)
    {
        builder.ToTable("content_documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Slug)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder.HasIndex(d => d.Slug)
            .IsUnique();

        builder.Property(d => d.Title)
            .HasColumnType("varchar(512)")
            .IsRequired();

        builder.Property(d => d.Summary)
            .HasColumnType("text");

        builder.Property(d => d.MarkdownBody)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(d => d.RenderedHtml)
            .HasColumnType("text");

        builder.Property(d => d.Status)
            .HasColumnType("varchar(20)")
            .HasConversion(new EnumToStringConverter<DocumentStatus>());

        builder.Property(d => d.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(d => d.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(d => d.PublishedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(d => d.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(d => d.DeletedBy)
            .HasColumnType("varchar(256)");

        builder.Property(d => d.IsArchived)
            .HasDefaultValue(false);

        builder.Property(d => d.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(d => d.CurrentVersion)
            .HasDefaultValue(1);

        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.UpdatedAt);
        builder.HasIndex(d => d.IsDeleted);

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Document)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.AuditTrail)
            .WithOne(a => a.Document)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Tags)
            .WithOne(t => t.Document)
            .HasForeignKey(t => t.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

