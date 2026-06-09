using KnowVaultCore.Domain.Enums;

namespace KnowVaultCore.Domain.Entities;

public class ContentDocument
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string MarkdownBody { get; set; } = string.Empty;
    public string RenderedHtml { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsArchived { get; set; }
    public int CurrentVersion { get; set; } = 1;

    public List<ContentDocumentVersion> Versions { get; set; } = new();
    public List<ContentDocumentAudit> AuditTrail { get; set; } = new();
    public List<ContentDocumentTag> Tags { get; set; } = new();
}

