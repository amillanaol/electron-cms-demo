using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KnowVaultCore.Infrastructure.Data.Configurations;

public class ContentDocumentAuditConfiguration : IEntityTypeConfiguration<ContentDocumentAudit>
{
    public void Configure(EntityTypeBuilder<ContentDocumentAudit> builder)
    {
        builder.ToTable("content_document_audits");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .HasColumnType("varchar(128)")
            .HasDefaultValue("ContentDocument");

        builder.Property(a => a.Action)
            .HasColumnType("varchar(20)")
            .HasConversion(new EnumToStringConverter<AuditAction>());

        builder.Property(a => a.PerformedBy)
            .HasColumnType("varchar(256)");

        builder.Property(a => a.Timestamp)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.ChangesJson)
            .HasColumnType("text");

        builder.HasIndex(a => a.DocumentId);
        builder.HasIndex(a => a.Timestamp);
    }
}

