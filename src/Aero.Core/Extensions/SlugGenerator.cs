namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for SlugGenerator.
/// </summary>
public static class SlugGenerator
{
        /// <summary>
    /// GenerateSlug method.
    /// </summary>
public static string GenerateSlug(this string title)
    {
        // todo - figure out this logic?!?! 
        if (string.IsNullOrEmpty(title))
            return Guid.NewGuid().ToString()[..8];

        // Basic slug generation
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("'", "")
            .Replace("\"", "");

        // Remove multiple dashes
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        // Trim dashes from start and end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 200)
        {
            slug = slug[..200].TrimEnd('-');
        }

        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString()[..8] : slug;
    }
}