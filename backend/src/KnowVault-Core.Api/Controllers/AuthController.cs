using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KnowVaultCore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KnowVaultCore.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IGroupRepository _groupRepo;

    public AuthController(IConfiguration config, IGroupRepository groupRepo)
    {
        _config = config;
        _groupRepo = groupRepo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "username and password are required" });

        var users = _config.GetSection("Users").Get<List<UserConfig>>();
        var user = users?.FirstOrDefault(u =>
            u.Username == request.Username && u.Password == request.Password);

        if (user is null)
            return Unauthorized(new { error = "invalid credentials" });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("name", user.Username),
            new("role", user.Role),
            new("group", user.Group),
        };

        var group = await _groupRepo.GetBySlugAsync(user.Group);
        if (group is not null)
        {
            foreach (var perm in group.Permissions)
            {
                claims.Add(new Claim("permission", $"{perm.Resource}:{perm.Action}"));
            }
        }

        var jwtKey = _config["Jwt:Secret"] ?? "default-dev-key-not-for-production-1234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = _config.GetValue<int?>("Jwt:ExpiryHours") ?? 24;

        var token = new JwtSecurityToken(
            issuer: "KnowVault-Core",
            audience: "KnowVault-Core",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiry),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            username = user.Username,
            role = user.Role
        });
    }
}

public record LoginRequest(string Username, string Password);

public class UserConfig
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
