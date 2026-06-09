using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KnowVaultCore.Api.Controllers;

[ApiController]
[Route("api/admin/groups")]
public class AdminController : ControllerBase
{
    private readonly IGroupRepository _groupRepo;
    private readonly ICurrentUser _currentUser;

    public AdminController(IGroupRepository groupRepo, ICurrentUser currentUser)
    {
        _groupRepo = groupRepo;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!_currentUser.HasPermission("admin", "manage-groups"))
            return Forbid();
        var groups = await _groupRepo.GetAllAsync();
        return Ok(groups.Select(g => new
        {
            g.Id, g.Name, g.Slug, g.CreatedAt,
            Permissions = g.Permissions.Select(p => new { p.Resource, p.Action })
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!_currentUser.HasPermission("admin", "manage-groups"))
            return Forbid();
        var group = await _groupRepo.GetByIdAsync(id);
        return group is null ? NotFound() : Ok(new
        {
            group.Id, group.Name, group.Slug, group.CreatedAt,
            Permissions = group.Permissions.Select(p => new { p.Resource, p.Action })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        if (!_currentUser.HasPermission("admin", "manage-groups"))
            return Forbid();
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
            return BadRequest(new { error = "name and slug are required" });

        var existing = await _groupRepo.GetBySlugAsync(request.Slug);
        if (existing is not null)
            return Conflict(new { error = "group slug already exists" });

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        if (request.Permissions is not null)
        {
            group.Permissions = request.Permissions.Select(p => new GroupPermission
            {
                Id = Guid.NewGuid(),
                Resource = p.Resource,
                Action = p.Action
            }).ToList();
        }

        await _groupRepo.AddAsync(group);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, new { group.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest request)
    {
        if (!_currentUser.HasPermission("admin", "manage-groups"))
            return Forbid();
        var group = await _groupRepo.GetByIdAsync(id);
        if (group is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
            group.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Slug))
            group.Slug = request.Slug.ToLowerInvariant();

        if (request.Permissions is not null)
        {
            group.Permissions.Clear();
            group.Permissions.AddRange(request.Permissions.Select(p => new GroupPermission
            {
                Id = Guid.NewGuid(),
                Resource = p.Resource,
                Action = p.Action
            }));
        }

        await _groupRepo.UpdateAsync(group);
        return Ok(new { message = "group updated" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!_currentUser.HasPermission("admin", "manage-groups"))
            return Forbid();
        await _groupRepo.DeleteAsync(id);
        return Ok(new { message = "group deleted" });
    }
}

public record CreateGroupRequest(string Name, string Slug, List<PermissionRequest>? Permissions);
public record UpdateGroupRequest(string? Name, string? Slug, List<PermissionRequest>? Permissions);
public record PermissionRequest(string Resource, string Action);
