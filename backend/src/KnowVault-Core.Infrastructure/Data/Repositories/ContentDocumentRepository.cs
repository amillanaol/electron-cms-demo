using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowVaultCore.Infrastructure.Data.Repositories;

public class ContentDocumentRepository : IContentDocumentRepository
{
    private readonly KnowVaultCoreDbContext _db;

    public ContentDocumentRepository(KnowVaultCoreDbContext db)
    {
        _db = db;
    }

    public async Task<ContentDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ContentDocuments
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<ContentDocument?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ContentDocuments
            .IgnoreQueryFilters()
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<ContentDocument?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.ContentDocuments
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Slug == slug, ct);
    }

    public async Task<List<ContentDocument>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.ContentDocuments
            .Include(d => d.Tags)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ContentDocument>> GetDeletedAsync(CancellationToken ct = default)
    {
        return await _db.ContentDocuments
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted)
            .OrderByDescending(d => d.DeletedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ContentDocument document, CancellationToken ct = default)
    {
        await _db.ContentDocuments.AddAsync(document, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ContentDocument document, CancellationToken ct = default)
    {
        _db.ContentDocuments.Update(document);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, string? deletedBy = null, CancellationToken ct = default)
    {
        var doc = await _db.ContentDocuments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doc is not null)
        {
            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;
            doc.DeletedBy = deletedBy;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _db.ContentDocuments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doc is not null)
        {
            _db.ContentDocuments.Remove(doc);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _db.ContentDocuments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doc is not null)
        {
            doc.IsDeleted = false;
            doc.DeletedAt = null;
            doc.DeletedBy = null;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<List<ContentDocumentVersion>> GetVersionsAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _db.ContentDocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);
    }

    public async Task<ContentDocumentVersion?> GetVersionByNumberAsync(Guid documentId, int versionNumber, CancellationToken ct = default)
    {
        return await _db.ContentDocumentVersions
            .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.VersionNumber == versionNumber, ct);
    }

    public async Task AddVersionAsync(ContentDocumentVersion version, CancellationToken ct = default)
    {
        await _db.ContentDocumentVersions.AddAsync(version, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnmarkCurrentVersionAsync(Guid documentId, CancellationToken ct = default)
    {
        var current = await _db.ContentDocumentVersions
            .Where(v => v.DocumentId == documentId && v.IsCurrent)
            .ToListAsync(ct);
        foreach (var v in current)
        {
            v.IsCurrent = false;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<ContentDocumentAudit>> GetAuditTrailAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _db.ContentDocumentAudits
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task AddAuditAsync(ContentDocumentAudit audit, CancellationToken ct = default)
    {
        await _db.ContentDocumentAudits.AddAsync(audit, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<ContentDocumentTag>> GetTagsAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _db.ContentDocumentTags
            .Where(t => t.DocumentId == documentId)
            .ToListAsync(ct);
    }

    public async Task SetTagsAsync(Guid documentId, List<string> tagNames, CancellationToken ct = default)
    {
        var existing = await _db.ContentDocumentTags
            .Where(t => t.DocumentId == documentId)
            .ToListAsync(ct);
        _db.ContentDocumentTags.RemoveRange(existing);

        await _db.ContentDocumentTags
            .AddRangeAsync(tagNames.Select(name => new ContentDocumentTag
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                Name = name.ToLowerInvariant().Trim()
            }), ct);
        await _db.SaveChangesAsync(ct);
    }
}

