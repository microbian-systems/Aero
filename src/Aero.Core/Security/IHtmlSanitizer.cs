namespace Aero.Core.Security;

public interface IHtmlSanitizer
{
    string Sanitize(string? html);
}
