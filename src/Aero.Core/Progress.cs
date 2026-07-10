using System.Collections;

namespace Aero.Core;

/// <summary>
/// Represents a class for Progress.
/// </summary>
public class Progress
{
        /// <summary>
    /// From method.
    /// </summary>
public static IEnumerable From(ICollection rgItems)
    {
        var _cItems = rgItems.Count;
        var nPercent = 0;

        var _iItem = 0;
        foreach (var o in rgItems)
        {
            yield return o;
            _iItem++;
            var nPercentNew = (int)(Math.Round(1.0 * _iItem / _cItems, 1) * 100);
            if (nPercentNew != nPercent)
            {
                nPercent = nPercentNew;
                Console.Error.Write("{0}% ", nPercent);
            }
        }
    }
}