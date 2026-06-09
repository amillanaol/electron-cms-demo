namespace KnowVaultCore.Domain.Entities;

public class ContentDocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MarkdownBody { get; set; } = string.Empty;
    public string RenderedHtml { get; set; } = string.Empty;
    public string? ChangeSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsCurrent { get; set; }

    public ContentDocument? Document { get; set; }
}

