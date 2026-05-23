namespace Aero.Core.Extensions;

public static class EnumerableExtensions
{
    public static string Concatenate(this IEnumerable<string> source, string separator = ", ")
        => string.Join(separator, source);

    public static string ConcatenateLines(this IEnumerable<string> source)
        => string.Join(Environment.NewLine, source);

    public static string Concatenate<T>(
        this IEnumerable<T> source,
        Func<T, string> selector,
        string separator = ", ")
        => string.Join(separator, source.Select(selector));

    public static string ConcatenateLines<T>(
        this IEnumerable<T> source,
        Func<T, string> selector)
        => string.Join(Environment.NewLine, source.Select(selector));

    // todo - requries installing FluentValidation package, which is a common dependency for validation in .NET apps, but may not be desired in this core library. Consider moving this to a separate package or module that depends on FluentValidation.
    // Fluent extension for converting validation errors to a single string message
    //public static string ToErrorString(this IEnumerable<ValidationFailure> errors)
    //    => errors.Concatenate(e => e.ErrorMessage);
}