namespace KnowVaultCore.Application.Interfaces;

public interface ICurrentUser
{
    string Name { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string resource, string action);
}
