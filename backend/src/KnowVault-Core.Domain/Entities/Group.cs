namespace KnowVaultCore.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<GroupPermission> Permissions { get; set; } = new();
}
