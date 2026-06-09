namespace KnowVaultCore.Domain.Entities;

public class ContentDocumentTag
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ContentDocument? Document { get; set; }
}

