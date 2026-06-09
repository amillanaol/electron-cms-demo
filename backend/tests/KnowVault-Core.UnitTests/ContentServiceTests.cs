using KnowVaultCore.Application.Dtos;
using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Application.Services;
using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Domain.Enums;
using Moq;

namespace KnowVaultCore.UnitTests;

public class ContentServiceTests
{
    private readonly Mock<IContentDocumentRepository> _repoMock = new();
    private readonly Mock<IMarkdownRenderer> _rendererMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly ContentService _service;

    public ContentServiceTests()
    {
        _rendererMock.Setup(r => r.Render(It.IsAny<string>())).Returns("<p>rendered</p>");
        _repoMock.Setup(r => r.GetTagsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentDocumentTag>());
        _repoMock.Setup(r => r.AddVersionAsync(It.IsAny<ContentDocumentVersion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<ContentDocumentAudit>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.UnmarkCurrentVersionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _currentUserMock.Setup(u => u.Name).Returns("test-user");
        _currentUserMock.Setup(u => u.Role).Returns("admin");
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _service = new ContentService(_repoMock.Object, _rendererMock.Object, _currentUserMock.Object);
    }

    private static ContentDocument MakeDoc(Guid id, string slug, string title, DocumentStatus status)
    {
        return new ContentDocument
        {
            Id = id,
            Slug = slug,
            Title = title,
            Summary = "summary",
            MarkdownBody = "# body",
            RenderedHtml = "<h1>body</h1>",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CurrentVersion = 1
        };
    }

    [Fact]
    public async Task GetPublishedAsync_ReturnsOnlyPublished()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "pub", "Published", DocumentStatus.Published),
            MakeDoc(Guid.NewGuid(), "draft", "Draft", DocumentStatus.Draft),
            MakeDoc(Guid.NewGuid(), "arch", "Archived", DocumentStatus.Archived)
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.GetPublishedAsync();

        Assert.Single(result);
        Assert.Equal("pub", result[0].Slug);
    }

    [Fact]
    public async Task GetPublishedAsync_EmptyList_WhenNoPublished()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "draft", "Draft", DocumentStatus.Draft)
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.GetPublishedAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsDoc_WhenFound()
    {
        var doc = MakeDoc(Guid.NewGuid(), "test-slug", "Test", DocumentStatus.Published);
        _repoMock.Setup(r => r.GetBySlugAsync("test-slug", It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var result = await _service.GetBySlugAsync("test-slug");

        Assert.NotNull(result);
        Assert.Equal("test-slug", result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetBySlugAsync("no-existe", It.IsAny<CancellationToken>())).ReturnsAsync((ContentDocument?)null);

        var result = await _service.GetBySlugAsync("no-existe");

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_FindsByTitle()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "guide", "Installation Guide", DocumentStatus.Published),
            MakeDoc(Guid.NewGuid(), "other", "Other Topic", DocumentStatus.Published)
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.SearchAsync("guide");

        Assert.Single(result);
        Assert.Equal("guide", result[0].Slug);
    }

    [Fact]
    public async Task SearchAsync_EmptyText_ReturnsEmpty()
    {
        var result = await _service.SearchAsync("");
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "a", "Alpha", DocumentStatus.Published)
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.SearchAsync("zzz");

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_StoresAndReturnsDocument()
    {
        var request = new CreateContentRequest("New Doc", "new-doc", "A summary", "# Body");
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ContentDocument>(), It.IsAny<CancellationToken>()))
            .Callback<ContentDocument, CancellationToken>((doc, _) => doc.Id = Guid.NewGuid())
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request);

        Assert.Equal("new-doc", result.Slug);
        Assert.Equal("New Doc", result.Title);
        Assert.Equal("Draft", result.Status);
        _rendererMock.Verify(r => r.Render("# Body"), Times.Once);
        _repoMock.Verify(r => r.AddVersionAsync(It.IsAny<ContentDocumentVersion>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<ContentDocumentAudit>(a => a.PerformedBy == "test-user"), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddVersionAsync(It.Is<ContentDocumentVersion>(v => v.CreatedBy == "test-user"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_SetsPublishedStatus()
    {
        var doc = MakeDoc(Guid.NewGuid(), "draft", "Draft Doc", DocumentStatus.Draft);
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var result = await _service.PublishAsync(doc.Id);

        Assert.NotNull(result);
        Assert.Equal("Published", result.Status);
        _repoMock.Verify(r => r.UpdateAsync(doc, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAuditAsync(It.IsAny<ContentDocumentAudit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ContentDocument?)null);

        var result = await _service.PublishAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveAsync_SetsArchivedStatus()
    {
        var doc = MakeDoc(Guid.NewGuid(), "pub", "Published Doc", DocumentStatus.Published);
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var result = await _service.ArchiveAsync(doc.Id);

        Assert.NotNull(result);
        Assert.Equal("Archived", result.Status);
        _repoMock.Verify(r => r.UpdateAsync(doc, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAuditAsync(It.IsAny<ContentDocumentAudit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ContentDocument?)null);

        var result = await _service.ArchiveAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesContentAndRendersMarkdown()
    {
        var doc = MakeDoc(Guid.NewGuid(), "old", "Old Title", DocumentStatus.Draft);
        doc.RenderedHtml = "<p>old</p>";
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var request = new UpdateContentRequest("New Title", "New summary", "# New body");
        var result = await _service.UpdateAsync(doc.Id, request);

        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        _rendererMock.Verify(r => r.Render("# New body"), Times.AtLeastOnce);
        _repoMock.Verify(r => r.UnmarkCurrentVersionAsync(doc.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddVersionAsync(It.IsAny<ContentDocumentVersion>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAuditAsync(It.IsAny<ContentDocumentAudit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ContentDocument?)null);

        var result = await _service.UpdateAsync(id, new UpdateContentRequest("T", "S", "B"));

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_OnlySearchesPublished()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "pub", "Hello World", DocumentStatus.Published),
            MakeDoc(Guid.NewGuid(), "draft", "Hello Draft", DocumentStatus.Draft)
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.SearchAsync("hello");

        Assert.Single(result);
        Assert.Equal("pub", result[0].Slug);
    }

    [Fact]
    public async Task Dto_DoesNotExposeMarkdownBody()
    {
        var doc = MakeDoc(Guid.NewGuid(), "test", "Test", DocumentStatus.Published);
        _repoMock.Setup(r => r.GetBySlugAsync("test", It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var result = await _service.GetBySlugAsync("test");
        var json = System.Text.Json.JsonSerializer.Serialize(result);

        Assert.DoesNotContain("markdownBody", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MarkdownBody", json);
    }

    [Fact]
    public async Task SoftDeleteAsync_DeletesAndAudits()
    {
        var doc = MakeDoc(Guid.NewGuid(), "del", "To Delete", DocumentStatus.Published);
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _repoMock.Setup(r => r.SoftDeleteAsync(doc.Id, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SoftDeleteAsync(doc.Id);

        Assert.True(result);
        _repoMock.Verify(r => r.SoftDeleteAsync(doc.Id, "test-user", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<ContentDocumentAudit>(a => a.Action == AuditAction.Delete), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ContentDocument?)null);

        var result = await _service.SoftDeleteAsync(id);

        Assert.False(result);
    }

    [Fact]
    public async Task GetDeletedAsync_ReturnsDeletedDocuments()
    {
        var docs = new List<ContentDocument>
        {
            MakeDoc(Guid.NewGuid(), "del1", "Deleted 1", DocumentStatus.Published),
            MakeDoc(Guid.NewGuid(), "del2", "Deleted 2", DocumentStatus.Draft)
        };
        _repoMock.Setup(r => r.GetDeletedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var result = await _service.GetDeletedAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetVersionsAsync_ReturnsVersions()
    {
        var versions = new List<ContentDocumentVersion>
        {
            new() { VersionNumber = 2, Title = "v2", ChangeSummary = "Update", CreatedAt = DateTime.UtcNow, IsCurrent = true },
            new() { VersionNumber = 1, Title = "v1", ChangeSummary = "Initial", CreatedAt = DateTime.UtcNow.AddDays(-1), IsCurrent = false }
        };
        var docId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetVersionsAsync(docId, It.IsAny<CancellationToken>())).ReturnsAsync(versions);

        var result = await _service.GetVersionsAsync(docId);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].VersionNumber);
    }

    [Fact]
    public async Task GetAuditTrailAsync_ReturnsAudits()
    {
        var audits = new List<ContentDocumentAudit>
        {
            new() { Action = AuditAction.Publish, Timestamp = DateTime.UtcNow },
            new() { Action = AuditAction.Create, Timestamp = DateTime.UtcNow.AddDays(-1) }
        };
        var docId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetAuditTrailAsync(docId, It.IsAny<CancellationToken>())).ReturnsAsync(audits);

        var result = await _service.GetAuditTrailAsync(docId);

        Assert.Equal(2, result.Count);
    }
}
