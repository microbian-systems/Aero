using System.Globalization;

namespace Aero.Core.Globalization;

public sealed record AeroLocaleOption(
    string CultureName,
    string DisplayName,
    string EnglishName,
    string NativeName,
    string LanguageName,
    string RegionCode,
    string RegionName,
    bool IsRightToLeft);

public static class AeroLocaleCatalog
{
    private static readonly Lazy<IReadOnlyList<AeroLocaleOption>> LocaleOptions = new(BuildLocaleOptions);

    public static IReadOnlyList<AeroLocaleOption> GetLocales() => LocaleOptions.Value;

    public static string NormalizeCultureOrDefault(string? culture, string fallback = "en-US")
    {
        if (string.IsNullOrWhiteSpace(culture))
            return fallback;

        try
        {
            return CultureInfo.GetCultureInfo(culture.Trim()).Name;
        }
        catch (CultureNotFoundException)
        {
            return fallback;
        }
    }

    private static IReadOnlyList<AeroLocaleOption> BuildLocaleOptions()
        => CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(CreateOption)
            .Where(option => option is not null)
            .Select(option => option!)
            .GroupBy(option => option.CultureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(option => option.RegionName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(option => option.LanguageName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(option => option.CultureName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static AeroLocaleOption? CreateOption(CultureInfo culture)
    {
        try
        {
            var region = new RegionInfo(culture.Name);
            return new AeroLocaleOption(
                culture.Name,
                culture.DisplayName,
                culture.EnglishName,
                culture.NativeName,
                culture.Parent.EnglishName,
                region.TwoLetterISORegionName,
                region.EnglishName,
                culture.TextInfo.IsRightToLeft);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
