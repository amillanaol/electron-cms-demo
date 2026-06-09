using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KnowVaultCore.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _adminClient;
    private readonly HttpClient _anonymousClient;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
        _adminClient = testFactory.CreateClient();
        _adminClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + GenerateAdminToken());

        _anonymousClient = testFactory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Equal("ok", body["status"]);
    }

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var response = await _adminClient.GetAsync("/api/ping");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Equal("pong", body["message"]);
    }

    [Fact]
    public async Task GetContent_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/content");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetContentBySlug_NotFound_Returns404()
    {
        var response = await _adminClient.GetAsync("/api/content/no-existe");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithoutText_Returns400()
    {
        var response = await _adminClient.GetAsync("/api/content/search");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithText_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/content/search?text=test");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateContent_ValidRequest_Returns201()
    {
        var slug = $"integration-test-doc-{Guid.NewGuid():n}";
        var request = new
        {
            title = "Integration Test Doc",
            slug,
            summary = "Created during integration test",
            markdownBody = "# Hello from test"
        };

        var response = await _adminClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal(slug, body["slug"].ToString());
    }

    [Fact]
    public async Task CreateContent_EmptyTitle_Returns400()
    {
        var request = new { title = "", slug = "test", markdownBody = "body" };
        var response = await _adminClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContent_EmptySlug_Returns400()
    {
        var request = new { title = "Title", slug = "", markdownBody = "body" };
        var response = await _adminClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContent_EmptyBody_Returns400()
    {
        var request = new { title = "Title", slug = "test", markdownBody = "" };
        var response = await _adminClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishContent_NotFound_Returns404()
    {
        var response = await _adminClient.PostAsync($"/api/content/{Guid.NewGuid()}/publish", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveContent_NotFound_Returns404()
    {
        var response = await _adminClient.PostAsync($"/api/content/{Guid.NewGuid()}/archive", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContent_NotFound_Returns404()
    {
        var request = new { title = "Updated", markdownBody = "# Updated" };
        var response = await _adminClient.PutAsJsonAsync($"/api/content/{Guid.NewGuid()}", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkdownRender_ValidMarkdown_ReturnsHtml()
    {
        var request = new { markdown = "# Hello" };
        var response = await _adminClient.PostAsJsonAsync("/api/markdown/render", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Contains("<h1>", body["html"]);
    }

    [Fact]
    public async Task MarkdownRender_EmptyBody_Returns400()
    {
        var request = new { markdown = "" };
        var response = await _adminClient.PostAsJsonAsync("/api/markdown/render", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MarkdownRender_Secure_JavascriptIsSanitized()
    {
        var request = new { markdown = "[xss](javascript:alert(1))" };
        var response = await _adminClient.PostAsJsonAsync("/api/markdown/render", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.DoesNotContain("javascript:", body["html"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkdownRender_Secure_RawHtmlEventHandlersAreEscaped()
    {
        var request = new { markdown = "<div onclick=\"alert(1)\">click</div>" };
        var response = await _adminClient.PostAsJsonAsync("/api/markdown/render", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Contains("onclick", body["html"]);
        Assert.Contains("&lt;div", body["html"]);
    }

    [Fact]
    public async Task CreateAndPublish_FullWorkflow()
    {
        var slug = $"workflow-test-{Guid.NewGuid():n}";
        var createRequest = new
        {
            title = "Workflow Test",
            slug,
            summary = "Testing full lifecycle",
            markdownBody = "# Workflow"
        };

        var createResp = await _adminClient.PostAsJsonAsync("/api/content", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(created);

        var id = Guid.Parse(created["id"].ToString()!);

        var publishResp = await _adminClient.PostAsync($"/api/content/{id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResp.StatusCode);

        var published = await publishResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(published);
        Assert.Equal("Published", published["status"].ToString());

        var getResp = await _adminClient.GetAsync($"/api/content/{slug}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
    }

    [Fact]
    public async Task CreateContent_WithoutAuth_ReturnsForbidden()
    {
        var slug = $"no-auth-test-{Guid.NewGuid():n}";
        var request = new
        {
            title = "No Auth Test",
            slug,
            summary = "Created without auth",
            markdownBody = "# Forbidden test"
        };

        var response = await _anonymousClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateContent_AsViewer_ReturnsForbidden()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.UseEnvironment("Development"));
        var viewerClient = factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + GenerateViewerToken());

        var slug = $"viewer-test-{Guid.NewGuid():n}";
        var request = new
        {
            title = "Viewer Test",
            slug,
            summary = "Created as viewer",
            markdownBody = "# Viewer test"
        };

        var response = await viewerClient.PostAsJsonAsync("/api/content", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static string GenerateToken(string username, string role, string group, string[]? permissions = null)
    {
        var jwtKey = "super-secret-key-not-for-production-12345678901234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("name", username),
            new("role", role),
            new("group", group),
        };

        if (permissions is not null)
        {
            foreach (var p in permissions)
                claims.Add(new Claim("permission", p));
        }

        var token = new JwtSecurityToken(
            issuer: "KnowVault-Core",
            audience: "KnowVault-Core",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateAdminToken()
    {
        return GenerateToken("admin", "admin", "admin", new[]
        {
            "content:create", "content:edit", "content:delete",
            "content:publish", "content:archive", "content:restore",
            "content:view-deleted", "admin:manage-groups"
        });
    }

    private static string GenerateViewerToken()
    {
        return GenerateToken("viewer", "viewer", "viewer");
    }
}
