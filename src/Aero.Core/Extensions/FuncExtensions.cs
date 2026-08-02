using System.Linq.Expressions;

namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for FuncExtensions.
/// </summary>
public static class FuncExtensions
{
        /// <summary>
    /// FuncToExpression method.
    /// </summary>
public static Expression<Func<T, bool>> FuncToExpression<T>(Func<T, bool> func)  
    {  
        return x => func(x);  
    } 
}