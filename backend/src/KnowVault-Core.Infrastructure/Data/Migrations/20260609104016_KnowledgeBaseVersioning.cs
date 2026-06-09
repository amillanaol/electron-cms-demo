using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowVaultCore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class KnowledgeBaseVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "varchar(256)", nullable: false),
                    Title = table.Column<string>(type: "varchar(512)", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    MarkdownBody = table.Column<string>(type: "text", nullable: false),
                    RenderedHtml = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(256)", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "content_document_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "varchar(128)", nullable: false, defaultValue: "ContentDocument"),
                    Action = table.Column<string>(type: "varchar(20)", nullable: false),
                    PerformedBy = table.Column<string>(type: "varchar(256)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangesJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_document_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_content_document_audits_content_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "content_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_document_tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_document_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_content_document_tags_content_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "content_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_document_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "varchar(512)", nullable: false),
                    MarkdownBody = table.Column<string>(type: "text", nullable: false),
                    RenderedHtml = table.Column<string>(type: "text", nullable: false),
                    ChangeSummary = table.Column<string>(type: "varchar(512)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(256)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_content_document_versions_content_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "content_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_content_document_audits_DocumentId",
                table: "content_document_audits",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_content_document_audits_Timestamp",
                table: "content_document_audits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_content_document_tags_DocumentId_Name",
                table: "content_document_tags",
                columns: new[] { "DocumentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_document_versions_DocumentId_IsCurrent",
                table: "content_document_versions",
                columns: new[] { "DocumentId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_content_document_versions_DocumentId_VersionNumber",
                table: "content_document_versions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_documents_IsDeleted",
                table: "content_documents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_content_documents_Slug",
                table: "content_documents",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_documents_Status",
                table: "content_documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_content_documents_UpdatedAt",
                table: "content_documents",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_document_audits");

            migrationBuilder.DropTable(
                name: "content_document_tags");

            migrationBuilder.DropTable(
                name: "content_document_versions");

            migrationBuilder.DropTable(
                name: "content_documents");
        }
    }
}

