using KnowVaultCore.Domain.Entities;

namespace KnowVaultCore.Application.Interfaces;

public interface IContentDocumentRepository
{
    Task<ContentDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContentDocument?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);
    Task<ContentDocument?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<ContentDocument>> GetAllAsync(CancellationToken ct = default);
    Task<List<ContentDocument>> GetDeletedAsync(CancellationToken ct = default);
    Task AddAsync(ContentDocument document, CancellationToken ct = default);
    Task UpdateAsync(ContentDocument document, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, string? deletedBy = null, CancellationToken ct = default);
    Task HardDeleteAsync(Guid id, CancellationToken ct = default);
    Task RestoreAsync(Guid id, CancellationToken ct = default);

    Task<List<ContentDocumentVersion>> GetVersionsAsync(Guid documentId, CancellationToken ct = default);
    Task<ContentDocumentVersion?> GetVersionByNumberAsync(Guid documentId, int versionNumber, CancellationToken ct = default);
    Task AddVersionAsync(ContentDocumentVersion version, CancellationToken ct = default);
    Task UnmarkCurrentVersionAsync(Guid documentId, CancellationToken ct = default);

    Task<List<ContentDocumentAudit>> GetAuditTrailAsync(Guid documentId, CancellationToken ct = default);
    Task AddAuditAsync(ContentDocumentAudit audit, CancellationToken ct = default);

    Task<List<ContentDocumentTag>> GetTagsAsync(Guid documentId, CancellationToken ct = default);
    Task SetTagsAsync(Guid documentId, List<string> tagNames, CancellationToken ct = default);
}

