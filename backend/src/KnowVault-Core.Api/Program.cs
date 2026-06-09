using System.Text;
using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Application.Services;
using KnowVaultCore.Infrastructure.Data;
using KnowVaultCore.Infrastructure.Data.Repositories;
using KnowVaultCore.Infrastructure.Data.Seed;
using KnowVaultCore.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["Jwt:Secret"] ?? "default-dev-key-not-for-production-1234567890";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "KnowVault-Core",
            ValidAudience = "KnowVault-Core",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IMarkdownRenderer, MarkdownRenderer>();
builder.Services.AddScoped<IContentDocumentRepository, ContentDocumentRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddDbContext<KnowVaultCoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var currentUser = context.RequestServices.GetRequiredService<ICurrentUser>();
        if (currentUser is CurrentUserService service)
        {
            var name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                       ?? context.User.FindFirst("name")?.Value ?? "anonymous";
            var role = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                       ?? context.User.FindFirst("role")?.Value ?? "";
            var permissions = context.User.FindAll("permission").Select(c => c.Value);
            service.SetUser(name, role, permissions);
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KnowVaultCoreDbContext>();
    await db.Database.MigrateAsync();
    await GroupSeeder.SeedGroupsAsync(db);

    if (!await db.ContentDocuments.AnyAsync())
    {
        var renderer = scope.ServiceProvider.GetRequiredService<IMarkdownRenderer>();
        var (seedDocs, seedVersions, seedAudits) = DataSeeder.GetSeedData(renderer);
        db.ContentDocuments.AddRange(seedDocs);
        db.ContentDocumentVersions.AddRange(seedVersions);
        db.ContentDocumentAudits.AddRange(seedAudits);
        await db.SaveChangesAsync();
    }
}

app.UseCors();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/ping", () => Results.Ok(new { message = "pong" }));
app.MapGet("/api/db/status", async (KnowVaultCoreDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    var docCount = canConnect ? await db.ContentDocuments.CountAsync() : 0;
    return canConnect
        ? Results.Ok(new { database = "connected", documentCount = docCount })
        : Results.Problem("Database unavailable", statusCode: 503);
});

app.MapPost("/api/markdown/render", (IMarkdownRenderer renderer, RenderRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Markdown))
        return Results.BadRequest(new { error = "markdown field is required" });
    var html = renderer.Render(req.Markdown);
    return Results.Ok(new { html });
});

app.Run();

public record RenderRequest(string Markdown);
