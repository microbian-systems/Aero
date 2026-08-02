namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for ExceptionExtensions.
/// </summary>
public static class ExceptionExtensions
{
        /// <summary>
    /// FromHierarchy method.
    /// </summary>
public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem,
        Func<TSource, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current))
        {
            yield return current;
        }
    }

        /// <summary>
    /// FromHierarchy method.
    /// </summary>
public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem)
        where TSource : class
    {
        return FromHierarchy(source, nextItem, s => s != null);
    }
        
        /// <summary>
    /// GetInnerExceptions method.
    /// </summary>
public static string GetInnerExceptions(this Exception exception)
    {
        var messages = exception.FromHierarchy(ex => ex.InnerException)
            .Select(ex => ex.Message);
        return String.Join(Environment.NewLine, messages);
    }
}