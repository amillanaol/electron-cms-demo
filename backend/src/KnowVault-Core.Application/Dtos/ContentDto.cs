namespace KnowVaultCore.Application.Dtos;

public record ContentDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Status,
    DateTime UpdatedAt,
    string? RenderedHtml,
    int CurrentVersion = 1,
    List<string>? Tags = null
);

public record CreateContentRequest(
    string Title,
    string Slug,
    string? Summary,
    string MarkdownBody,
    List<string>? Tags = null
);

public record UpdateContentRequest(
    string Title,
    string? Summary,
    string MarkdownBody,
    string? ChangeSummary = null
);

public record VersionDto(
    int VersionNumber,
    string Title,
    string? ChangeSummary,
    DateTime CreatedAt,
    string? CreatedBy,
    bool IsCurrent
);

public record AuditDto(
    string Action,
    string? PerformedBy,
    DateTime Timestamp,
    string? ChangesJson
);

public record RestoreRequest(
    int? VersionNumber
);

