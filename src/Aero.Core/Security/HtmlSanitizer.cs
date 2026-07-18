using System.Text.RegularExpressions;

namespace Aero.Core.Security;

/// <summary>
/// Represents a class for HtmlSanitizer.
/// </summary>
public sealed class HtmlSanitizer : IHtmlSanitizer
{
    private static readonly Regex ScriptTag =
        new(@"<script[^>]*>.*?</script>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex JavascriptScheme =
        new(@"\bjavascript\s*:",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OnEventAttribute =
        new(@"\son\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
    /// Sanitize method.
    /// </summary>
public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        html = ScriptTag.Replace(html, "");
        html = JavascriptScheme.Replace(html, "blocked:");
        html = OnEventAttribute.Replace(html, "");

        return html;
    }
}
