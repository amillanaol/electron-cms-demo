using KnowVaultCore.Domain.Entities;

namespace KnowVaultCore.Application.Interfaces;

public interface IGroupRepository
{
    Task<List<Group>> GetAllAsync(CancellationToken ct = default);
    Task<Group?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Group group, CancellationToken ct = default);
    Task UpdateAsync(Group group, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
