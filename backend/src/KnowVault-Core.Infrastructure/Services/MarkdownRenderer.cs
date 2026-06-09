using System.Text.RegularExpressions;
using KnowVaultCore.Application.Interfaces;
using Markdig;

namespace KnowVaultCore.Infrastructure.Services;

public partial class MarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderer()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseAutoLinks()
            .Build();
    }

    public string Render(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var html = Markdown.ToHtml(markdown, _pipeline);
        return Sanitize(html);
    }

    private static string Sanitize(string html)
    {
        html = DangerousSchemeRegex().Replace(html, m =>
        {
            var attr = m.Groups[1].Value;
            return $"{attr}about:blank";
        });
        html = EventHandlerAttrRegex().Replace(html, "");
        html = ExtraDangerousSchemeRegex().Replace(html, m =>
        {
            var attr = m.Groups[1].Value;
            return $"{attr}about:blank";
        });
        return html;
    }

    [GeneratedRegex(@"(href|src)\s*=\s*""\s*javascript\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex DangerousSchemeRegex();

    [GeneratedRegex(@"\s+on\w+\s*=\s*""[^""]*""", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerAttrRegex();

    [GeneratedRegex(@"(href|src)\s*=\s*""\s*vbscript\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex ExtraDangerousSchemeRegex();
}

