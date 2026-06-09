using KnowVaultCore.Infrastructure.Services;

namespace KnowVaultCore.UnitTests;

public class MarkdownRendererTests
{
    private readonly MarkdownRenderer _renderer = new();

    [Fact]
    public void Render_NullInput_ReturnsEmpty()
    {
        var result = _renderer.Render(null!);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Render_EmptyString_ReturnsEmpty()
    {
        var result = _renderer.Render("");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Render_Whitespace_ReturnsEmpty()
    {
        var result = _renderer.Render("   ");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Render_PlainText_WrapsInParagraph()
    {
        var result = _renderer.Render("Hello world");
        Assert.Contains("Hello world", result);
    }

    [Fact]
    public void Render_Heading_GeneratesH1()
    {
        var result = _renderer.Render("# Title");
        Assert.Contains("<h1>", result);
        Assert.Contains("Title", result);
    }

    [Fact]
    public void Render_BoldText_GeneratesStrong()
    {
        var result = _renderer.Render("**bold**");
        Assert.Contains("<strong>", result);
        Assert.Contains("bold", result);
    }

    [Fact]
    public void Sanitize_BlocksJavascriptInHref()
    {
        var result = _renderer.Render("[click](javascript:alert('xss'))");
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("about:blank", result);
    }

    [Fact]
    public void Sanitize_RawHtmlJavascriptInSrc_IsEscapedByDisableHtml()
    {
        var result = _renderer.Render("<img src=\"javascript:alert('xss')\">");
        Assert.Contains("javascript", result);
        Assert.DoesNotContain("<img", result);
    }

    [Fact]
    public void Sanitize_RawHtmlEventHandlers_AreEscapedByDisableHtml()
    {
        var result = _renderer.Render("<div onclick=\"alert('xss')\">content</div>");
        Assert.Contains("onclick", result);
        Assert.DoesNotContain("<div", result);
    }

    [Fact]
    public void Sanitize_RawHtmlOnError_IsEscapedByDisableHtml()
    {
        var result = _renderer.Render("<img src=\"x\" onerror=\"alert(1)\">");
        Assert.Contains("onerror", result);
        Assert.DoesNotContain("<img", result);
    }

    [Fact]
    public void DisableHtml_RawHtmlTagsAreEscaped()
    {
        var result = _renderer.Render("<script>alert('xss')</script>");
        Assert.Contains("&lt;script&gt;", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void DisableHtml_ArbitraryTagsAreEscaped()
    {
        var result = _renderer.Render("<iframe src=\"https://evil.com\"></iframe>");
        Assert.Contains("&lt;iframe", result);
        Assert.DoesNotContain("<iframe", result);
        Assert.DoesNotContain("<iframe ", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AutoLinks_UrlBecomesLink()
    {
        var result = _renderer.Render("https://example.com");
        Assert.Contains("<a href=\"https://example.com\"", result);
    }

    [Fact]
    public void Sanitize_UpperCaseJavascript_IsBlocked()
    {
        var result = _renderer.Render("[click](JAVASCRIPT:alert('xss'))");
        Assert.DoesNotContain("JAVASCRIPT:", result);
        Assert.Contains("about:blank", result);
    }

    [Fact]
    public void Sanitize_MixedCaseJavascript_IsBlocked()
    {
        var result = _renderer.Render("[click](JaVaScRiPt:alert('xss'))");
        Assert.DoesNotContain("JaVaScRiPt:", result);
        Assert.Contains("about:blank", result);
    }

    [Fact]
    public void Render_CodeBlock_IsPreserved()
    {
        var result = _renderer.Render("```\ncode\n```");
        Assert.Contains("code", result);
    }

    [Fact]
    public void Render_UnorderedList_GeneratesUl()
    {
        var result = _renderer.Render("- item1\n- item2");
        Assert.Contains("<ul>", result);
        Assert.Contains("item1", result);
        Assert.Contains("item2", result);
    }
}

