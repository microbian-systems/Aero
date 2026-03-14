namespace Aero.Core.Railway;

/// <summary>
/// Represents a computation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="TError">The type of the error value in case of failure.</typeparam>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public abstract record Result<TError, TValue>
{
    public sealed record Ok(TValue Value) : Result<TError, TValue>;
    public sealed record Failure(TError Error) : Result<TError, TValue>;
    
    public static implicit operator Result<TError, TValue>(TValue value) => new Ok(value);
    public static implicit operator Result<TError, TValue>(TError error) => new Failure(error);

    public static explicit operator TValue(Result<TError, TValue> result) =>
        result switch
        {
            Ok(var value) => value,
            Failure(var error) => throw new InvalidCastException($"Result was Failure: {error}"),
        };

    public static explicit operator TError(Result<TError, TValue> result) =>
        result switch
        {
            Failure(var error) => error,
            Ok(var value) => throw new InvalidCastException($"Result was Ok: {value}"),
        };
}

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the value that may be present.</typeparam>
public abstract record Option<T>
{
    public sealed record Some(T Value) : Option<T>;
    public sealed record None : Option<T>;

    /// <summary>
    /// Gets a value indicating whether this Option contains a value (Some case).
    /// </summary>
    public bool IsSome => this is Some;

    /// <summary>
    /// Gets a value indicating whether this Option has no value (None case).
    /// </summary>
    public bool IsNone => this is None;
    
    public static implicit operator Option<T>(T value) =>
        value is not null ? new Some(value) : new None();

    public static implicit operator Option<T>(Railway.None _) => new None();

    public static explicit operator T(Option<T> option) =>
        option switch
        {
            Some(var value) => value,
            None => throw new InvalidCastException("Cannot cast None to value"),
        };
}

/// <summary>
/// Represents the absence of a value, used primarily for type inference in generic contexts.
/// </summary>
public readonly struct None { }
