using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowVaultCore.Infrastructure.Data.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly KnowVaultCoreDbContext _db;

    public GroupRepository(KnowVaultCoreDbContext db)
    {
        _db = db;
    }

    public async Task<List<Group>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Groups.Include(g => g.Permissions).ToListAsync(ct);
    }

    public async Task<Group?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Groups.Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Slug == slug, ct);
    }

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Groups.Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task AddAsync(Group group, CancellationToken ct = default)
    {
        await _db.Groups.AddAsync(group, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Group group, CancellationToken ct = default)
    {
        _db.Groups.Update(group);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var group = await _db.Groups.FindAsync(new object[] { id }, ct);
        if (group is not null)
        {
            _db.Groups.Remove(group);
            await _db.SaveChangesAsync(ct);
        }
    }
}
