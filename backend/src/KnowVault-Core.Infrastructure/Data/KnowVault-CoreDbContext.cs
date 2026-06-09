using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace KnowVaultCore.Infrastructure.Data;

public class KnowVaultCoreDbContext : DbContext
{
    public KnowVaultCoreDbContext(DbContextOptions<KnowVaultCoreDbContext> options) : base(options)
    {
    }

    public DbSet<ContentDocument> ContentDocuments => Set<ContentDocument>();
    public DbSet<ContentDocumentVersion> ContentDocumentVersions => Set<ContentDocumentVersion>();
    public DbSet<ContentDocumentAudit> ContentDocumentAudits => Set<ContentDocumentAudit>();
    public DbSet<ContentDocumentTag> ContentDocumentTags => Set<ContentDocumentTag>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContentDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ContentDocumentVersionConfiguration());
        modelBuilder.ApplyConfiguration(new ContentDocumentAuditConfiguration());
        modelBuilder.ApplyConfiguration(new ContentDocumentTagConfiguration());
        modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new GroupPermissionConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

