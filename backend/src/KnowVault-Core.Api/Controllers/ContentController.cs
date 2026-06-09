using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Application.Dtos;
using KnowVaultCore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowVaultCore.Api.Controllers;

[ApiController]
[Route("api/content")]
public class ContentController : ControllerBase
{
    private readonly ContentService _service;
    private readonly ICurrentUser _currentUser;

    public ContentController(ContentService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _service.GetPublishedAsync();
        return Ok(docs);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var doc = await _service.GetBySlugAsync(slug);
        return doc is null ? NotFound(new { error = "Document not found" }) : Ok(doc);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "text query parameter is required" });
        var docs = await _service.SearchAsync(text);
        return Ok(docs);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContentRequest request)
    {
        if (!_currentUser.HasPermission("content", "create"))
            return Forbid();
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "title is required" });
        if (string.IsNullOrWhiteSpace(request.Slug))
            return BadRequest(new { error = "slug is required" });
        if (string.IsNullOrWhiteSpace(request.MarkdownBody))
            return BadRequest(new { error = "markdownBody is required" });

        var doc = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetBySlug), new { slug = doc.Slug }, doc);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentRequest request)
    {
        if (!_currentUser.HasPermission("content", "edit"))
            return Forbid();
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "title is required" });
        if (string.IsNullOrWhiteSpace(request.MarkdownBody))
            return BadRequest(new { error = "markdownBody is required" });

        var doc = await _service.UpdateAsync(id, request);
        return doc is null ? NotFound(new { error = "Document not found" }) : Ok(doc);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        if (!_currentUser.HasPermission("content", "publish"))
            return Forbid();
        var doc = await _service.PublishAsync(id);
        return doc is null ? NotFound(new { error = "Document not found" }) : Ok(doc);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        if (!_currentUser.HasPermission("content", "archive"))
            return Forbid();
        var doc = await _service.ArchiveAsync(id);
        return doc is null ? NotFound(new { error = "Document not found" }) : Ok(doc);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!_currentUser.HasPermission("content", "delete"))
            return Forbid();
        var ok = await _service.SoftDeleteAsync(id);
        return ok ? Ok(new { message = "Document deleted" }) : NotFound(new { error = "Document not found" });
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, [FromBody] RestoreRequest? request = null)
    {
        if (!_currentUser.HasPermission("content", "restore"))
            return Forbid();
        var doc = await _service.RestoreAsync(id, request?.VersionNumber);
        return doc is null ? NotFound(new { error = "Document not found" }) : Ok(doc);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        if (!_currentUser.HasPermission("content", "view-deleted"))
            return Forbid();
        var docs = await _service.GetDeletedAsync();
        return Ok(docs);
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var versions = await _service.GetVersionsAsync(id);
        return Ok(versions);
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAuditTrail(Guid id)
    {
        var audits = await _service.GetAuditTrailAsync(id);
        return Ok(audits);
    }
}
