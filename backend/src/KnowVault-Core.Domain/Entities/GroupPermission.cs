namespace KnowVaultCore.Domain.Entities;

public class GroupPermission
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public Group? Group { get; set; }
}
