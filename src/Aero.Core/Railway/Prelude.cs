namespace Aero.Core.Railway;

/// <summary>
/// Provides factory methods for creating <see cref="Option{T}"/> and <see cref="Result{TError, TValue}"/> values,
/// enabling functional construction patterns.
/// </summary>
/// <remarks>
/// <para>
/// The Prelude class serves as a convenience module for creating Option and Result values
/// in a functional style. It provides explicit factory methods that can be used when implicit
/// conversions are not desirable or for clarity in complex expressions.
/// </para>
/// <para>
/// <b>Lifting Values into Monadic Context:</b>
/// In functional programming, "lifting" refers to taking a value from the "normal" world
/// and placing it into a monadic context (like Option or Result). The Prelude methods
/// are the primary way to lift values:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="Some{T}(T)"/> lifts a value into the Option context as Some
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="None"/> lifts "absence" into the Option context
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Ok{TError, TValue}(TValue)"/> lifts a value into the Result context as success
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="Fail{TError, TValue}(TError)"/> lifts an error into the Result context as failure
/// </description>
/// </item>
/// </list>
/// <para>
/// These methods are particularly useful in scenarios where type inference needs help,
/// or when you want to be explicit about creating these values for code readability.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Explicit construction with Prelude
/// var someValue = Prelude.Some(42);
/// var noneValue = Prelude.None;
/// var success = Prelude.Ok&lt;string, int&gt;(100);
/// var failure = Prelude.Fail&lt;string, int&gt;("Error occurred");
/// 
/// // Using in expressions where type inference needs help
/// var results = items.Select(item =>
///     item.IsValid 
///         ? Prelude.Ok&lt;ValidationError, Item&gt;(item)
///         : Prelude.Fail&lt;ValidationError, Item&gt;(new ValidationError("Invalid item"))
/// );
/// </code>
/// </example>
public static class Prelude
{
    /// <summary>
    /// Creates an Option containing the specified value (Some case).
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in a Some.</param>
    /// <returns>An Option.Some containing the value.</returns>
    /// <remarks>
    /// This method explicitly lifts a value into the Option monadic context as a Some.
    /// It is equivalent to the implicit conversion but makes the intent clearer.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; name = Prelude.Some("Alice");
    /// // Equivalent to: Option&lt;string&gt; name = "Alice";
    /// </code>
    /// </example>
    public static Option<T> Some<T>(T value) => value;

    /// <summary>
    /// Represents the absence of a value (None case) for use in Option.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient way to create a None value without specifying
    /// the type parameter explicitly. It "lifts" the concept of "nothing" into the Option context.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; name = Prelude.None;
    /// // Equivalent to: Option&lt;string&gt; name = new Option&lt;string&gt;.None();
    /// </code>
    /// </example>
    public static readonly NoneType None = default;
    
    /// <summary>
    /// Creates a Result representing success with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error (for type inference).</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>A Result.Ok containing the value.</returns>
    /// <remarks>
    /// This method explicitly lifts a value into the Result monadic context as an Ok.
    /// It is useful when type inference needs explicit type parameters or for clarity.
    /// </remarks>
    public static Result<TValue, TError> Ok<TValue, TError>(TValue value)
        where TError : AeroError => 
        new Result<TValue, TError>.Ok(value);
        
    /// <summary>
    /// Creates a Result representing failure with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value (for type inference).</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="error">The error value.</param>
    /// <returns>A Result.Failure containing the error.</returns>
    /// <remarks>
    /// This method explicitly lifts an error into the Result monadic context as a Failure.
    /// It is useful when type inference needs explicit type parameters or for clarity.
    /// </remarks>
    public static Result<TValue, TError> Fail<TValue, TError>(TError error)
        where TError : AeroError => 
        new Result<TValue, TError>.Failure(error);

    // Todo - finish implementing prelude methods for default Result<T> with AeroError as the error type. This will be a common case and
    // having these factory methods can improve readability and reduce boilerplate when working with Result<T> where the error type is always AeroError.

    // Default Result<T> (AeroError)
    //public static Result<TValue> Ok<TValue>(TValue value) =>
    //    new Result<TValue, AeroEr>.Ok(value);

    //public static Result<TValue> Fail<TValue>(AeroError error) =>
    //    new Result<TValue>.Failure(error);
}