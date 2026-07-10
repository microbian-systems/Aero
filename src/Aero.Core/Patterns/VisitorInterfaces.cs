namespace Aero.Core.Patterns;

/// <summary>
/// Defines an interface for IVisitable.
/// </summary>
public interface IVisitable
{
        /// <summary>
    /// Accept method.
    /// </summary>
void Accept(IVisitor visitor);
}

/// <summary>
/// Defines an interface for IVisitable.
/// </summary>
public interface IVisitable<out TReturn>
{
        /// <summary>
    /// Accept method.
    /// </summary>
TReturn Accept(IVisitor visitor);
}

/// <summary>
/// Defines an interface for IVisitor.
/// </summary>
public interface IVisitor
{
        /// <summary>
    /// Visit method.
    /// </summary>
void Visit(object visited);
}
    
/// <summary>
/// Visits and potentially modifies a type T
/// </summary>
/// <typeparam name="T">Any type to be visited</typeparam>
public interface IVisitor<in T> : IVisitor
{
        /// <summary>
    /// Visit method.
    /// </summary>
void Visit(T visited);
}

/// <summary>
/// Visits a type T and returns type TReturn
/// </summary>
/// <typeparam name="T">Type to be visited</typeparam>
/// <typeparam name="TReturn">any type that is desired. (try with tuples)</typeparam>
public interface IVisitor<in T, out TReturn> : IVisitor
{
        /// <summary>
    /// Visit method.
    /// </summary>
TReturn Visit(T visited);
}