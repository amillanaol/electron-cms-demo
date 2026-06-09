using KnowVaultCore.Application.Dtos;
using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Domain.Enums;

namespace KnowVaultCore.Application.Services;

public class ContentService
{
    private readonly IContentDocumentRepository _repo;
    private readonly IMarkdownRenderer _renderer;
    private readonly ICurrentUser _currentUser;

    public ContentService(IContentDocumentRepository repo, IMarkdownRenderer renderer, ICurrentUser currentUser)
    {
        _repo = repo;
        _renderer = renderer;
        _currentUser = currentUser;
    }

    private string CurrentUser => _currentUser.IsAuthenticated ? _currentUser.Name : "anonymous";

    public async Task<List<ContentDto>> GetPublishedAsync()
    {
        var docs = await _repo.GetAllAsync();
        return docs
            .Where(d => d.Status == DocumentStatus.Published)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ContentDto?> GetBySlugAsync(string slug)
    {
        var doc = await _repo.GetBySlugAsync(slug);
        return doc is null ? null : await MapToDetailDto(doc);
    }

    public async Task<List<ContentDto>> SearchAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var docs = await _repo.GetAllAsync();
        var lower = text.ToLowerInvariant();
        return docs
            .Where(d => d.Status == DocumentStatus.Published)
            .Where(d => d.Title.Contains(lower, StringComparison.OrdinalIgnoreCase)
                        || d.Summary.Contains(lower, StringComparison.OrdinalIgnoreCase)
                        || d.Slug.Contains(lower, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ContentDto> CreateAsync(CreateContentRequest request)
    {
        var now = DateTime.UtcNow;
        var rendered = _renderer.Render(request.MarkdownBody);

        var doc = new ContentDocument
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Title = request.Title,
            Summary = request.Summary ?? string.Empty,
            MarkdownBody = request.MarkdownBody,
            RenderedHtml = rendered,
            Status = DocumentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
            CurrentVersion = 1
        };

        await _repo.AddAsync(doc);
        await _repo.AddVersionAsync(new ContentDocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            VersionNumber = 1,
            Title = doc.Title,
            MarkdownBody = doc.MarkdownBody,
            RenderedHtml = doc.RenderedHtml,
            ChangeSummary = "Creación inicial",
            CreatedAt = now,
            CreatedBy = CurrentUser,
            IsCurrent = true
        });

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Create,
            PerformedBy = CurrentUser,
            Timestamp = now
        });

        if (request.Tags is { Count: > 0 })
        {
            await _repo.SetTagsAsync(doc.Id, request.Tags);
        }

        return await MapToDetailDto(doc);
    }

    public async Task<ContentDto?> UpdateAsync(Guid id, UpdateContentRequest request)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc is null) return null;

        var now = DateTime.UtcNow;
        var prevVersion = doc.CurrentVersion;
        var newVersion = prevVersion + 1;

        await _repo.UnmarkCurrentVersionAsync(doc.Id);

        await _repo.AddVersionAsync(new ContentDocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            VersionNumber = newVersion,
            Title = request.Title,
            MarkdownBody = request.MarkdownBody,
            RenderedHtml = _renderer.Render(request.MarkdownBody),
            ChangeSummary = request.ChangeSummary ?? $"Actualización a versión {newVersion}",
            CreatedAt = now,
            CreatedBy = CurrentUser,
            IsCurrent = true
        });

        doc.Title = request.Title;
        doc.Summary = request.Summary ?? string.Empty;
        doc.MarkdownBody = request.MarkdownBody;
        doc.RenderedHtml = _renderer.Render(request.MarkdownBody);
        doc.UpdatedAt = now;
        doc.CurrentVersion = newVersion;
        await _repo.UpdateAsync(doc);

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Update,
            PerformedBy = CurrentUser,
            Timestamp = now,
            ChangesJson = $"{{\"version\": {newVersion}}}"
        });

        return await MapToDetailDto(doc);
    }

    public async Task<ContentDto?> PublishAsync(Guid id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc is null) return null;

        var now = DateTime.UtcNow;
        doc.Status = DocumentStatus.Published;
        doc.PublishedAt ??= now;
        doc.UpdatedAt = now;
        await _repo.UpdateAsync(doc);

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Publish,
            PerformedBy = CurrentUser,
            Timestamp = now
        });

        return await MapToDetailDto(doc);
    }

    public async Task<ContentDto?> ArchiveAsync(Guid id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc is null) return null;

        doc.Status = DocumentStatus.Archived;
        doc.IsArchived = true;
        doc.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(doc);

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Archive,
            PerformedBy = CurrentUser,
            Timestamp = DateTime.UtcNow
        });

        return await MapToDetailDto(doc);
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc is null) return false;

        await _repo.SoftDeleteAsync(id, CurrentUser);

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Delete,
            PerformedBy = CurrentUser,
            Timestamp = DateTime.UtcNow
        });

        return true;
    }

    public async Task<ContentDto?> RestoreAsync(Guid id, int? versionNumber = null)
    {
        var doc = await _repo.GetByIdIncludingDeletedAsync(id);
        if (doc is null) return null;

        var now = DateTime.UtcNow;

        if (versionNumber is not null)
        {
            var version = await _repo.GetVersionByNumberAsync(id, versionNumber.Value);
            if (version is not null)
            {
                var newVersion = doc.CurrentVersion + 1;

                await _repo.UnmarkCurrentVersionAsync(doc.Id);

                await _repo.AddVersionAsync(new ContentDocumentVersion
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    VersionNumber = newVersion,
                    Title = version.Title,
                    MarkdownBody = version.MarkdownBody,
                    RenderedHtml = version.RenderedHtml,
                    ChangeSummary = $"Restaurado desde versión {version.VersionNumber}",
                    CreatedAt = now,
                    CreatedBy = CurrentUser,
                    IsCurrent = true
                });

                doc.Title = version.Title;
                doc.MarkdownBody = version.MarkdownBody;
                doc.RenderedHtml = version.RenderedHtml;
                doc.CurrentVersion = newVersion;
            }
        }

        await _repo.RestoreAsync(id);
        doc.UpdatedAt = now;
        doc.Status = DocumentStatus.Draft;
        await _repo.UpdateAsync(doc);

        await _repo.AddAuditAsync(new ContentDocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = AuditAction.Restore,
            PerformedBy = CurrentUser,
            Timestamp = now,
            ChangesJson = versionNumber is not null
                ? $"{{\"restoredFromVersion\": {versionNumber}}}"
                : null
        });

        return await MapToDetailDto(doc);
    }

    public async Task<List<ContentDto>> GetDeletedAsync()
    {
        var docs = await _repo.GetDeletedAsync();
        return docs.Select(MapToDto).ToList();
    }

    public async Task<List<VersionDto>> GetVersionsAsync(Guid id)
    {
        var versions = await _repo.GetVersionsAsync(id);
        return versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new VersionDto(v.VersionNumber, v.Title, v.ChangeSummary, v.CreatedAt, v.CreatedBy, v.IsCurrent))
            .ToList();
    }

    public async Task<List<AuditDto>> GetAuditTrailAsync(Guid id)
    {
        var audits = await _repo.GetAuditTrailAsync(id);
        return audits
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditDto(a.Action.ToString(), a.PerformedBy, a.Timestamp, a.ChangesJson))
            .ToList();
    }

    private async Task<ContentDto> MapToDetailDto(ContentDocument doc)
    {
        var tags = (await _repo.GetTagsAsync(doc.Id)) ?? new List<ContentDocumentTag>();
        return new ContentDto(
            doc.Id,
            doc.Slug,
            doc.Title,
            doc.Summary,
            doc.Status.ToString(),
            doc.UpdatedAt,
            doc.RenderedHtml,
            doc.CurrentVersion,
            tags.Select(t => t.Name).ToList()
        );
    }

    private static ContentDto MapToDto(ContentDocument doc)
    {
        return new ContentDto(
            doc.Id,
            doc.Slug,
            doc.Title,
            doc.Summary,
            doc.Status.ToString(),
            doc.UpdatedAt,
            doc.RenderedHtml
        );
    }
}

