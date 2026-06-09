using KnowVaultCore.Domain.Enums;

namespace KnowVaultCore.Domain.Entities;

public class ContentDocumentAudit
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string EntityName { get; set; } = "ContentDocument";
    public AuditAction Action { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ChangesJson { get; set; }

    public ContentDocument? Document { get; set; }
}

