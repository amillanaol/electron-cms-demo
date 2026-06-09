using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowVaultCore.Infrastructure.Data.Seed;

public static class GroupSeeder
{
    public static async Task SeedGroupsAsync(KnowVaultCoreDbContext db)
    {
        if (await db.Groups.AnyAsync()) return;

        var now = DateTime.UtcNow;

        var adminGroup = new Group
        {
            Id = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
            Name = "Administradores",
            Slug = "admin",
            CreatedAt = now,
            Permissions = new List<GroupPermission>
            {
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000001"), Resource = "content", Action = "create" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000002"), Resource = "content", Action = "edit" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000003"), Resource = "content", Action = "delete" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000004"), Resource = "content", Action = "publish" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000005"), Resource = "content", Action = "archive" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000006"), Resource = "content", Action = "restore" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000007"), Resource = "content", Action = "view-deleted" },
                new() { Id = Guid.Parse("b1000000-0000-0000-0000-000000000008"), Resource = "admin", Action = "manage-groups" },
            }
        };

        var editorGroup = new Group
        {
            Id = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
            Name = "Editores",
            Slug = "editor",
            CreatedAt = now,
            Permissions = new List<GroupPermission>
            {
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000001"), Resource = "content", Action = "create" },
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000002"), Resource = "content", Action = "edit" },
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000003"), Resource = "content", Action = "publish" },
                new() { Id = Guid.Parse("b2000000-0000-0000-0000-000000000004"), Resource = "content", Action = "archive" },
            }
        };

        var viewerGroup = new Group
        {
            Id = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
            Name = "Lectores",
            Slug = "viewer",
            CreatedAt = now,
            Permissions = new List<GroupPermission>()
        };

        db.Groups.AddRange(adminGroup, editorGroup, viewerGroup);
        await db.SaveChangesAsync();
    }
}
