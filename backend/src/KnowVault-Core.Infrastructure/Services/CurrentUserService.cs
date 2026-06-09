using KnowVaultCore.Application.Interfaces;

namespace KnowVaultCore.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    public string Name { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsAuthenticated => !string.IsNullOrEmpty(Name);

    private readonly HashSet<string> _permissions = new();

    public void SetUser(string name, string role = "", IEnumerable<string>? permissions = null)
    {
        Name = name;
        Role = role;
        _permissions.Clear();
        if (permissions is not null)
        {
            foreach (var p in permissions) _permissions.Add(p);
        }
    }

    public bool HasPermission(string resource, string action) =>
        _permissions.Contains($"{resource}:{action}");
}
