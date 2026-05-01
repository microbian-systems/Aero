using System.Linq.Expressions;

namespace Aero.Core.Extensions;

public static class FuncExtensions
{
    public static Expression<Func<T, bool>> FuncToExpression<T>(Func<T, bool> func)  
    {  
        return x => func(x);  
    } 
}