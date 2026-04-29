using Ganss.Xss;

namespace Aero.Core.Security;

public sealed class HtmlSanitizer : IHtmlSanitizer
{
    private readonly Ganss.Xss.HtmlSanitizer sanitizer;

    public HtmlSanitizer()
    {
        sanitizer = new Ganss.Xss.HtmlSanitizer();
        sanitizer.AllowedTags.Remove("script");
        sanitizer.AllowedTags.Remove("style");
        sanitizer.AllowedSchemes.Remove("javascript");

        foreach (var attribute in sanitizer.AllowedAttributes.Where(static attribute => attribute.StartsWith("on", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            sanitizer.AllowedAttributes.Remove(attribute);
        }
    }

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return sanitizer.Sanitize(html);
    }
}
