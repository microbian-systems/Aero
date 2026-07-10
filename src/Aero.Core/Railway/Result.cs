namespace Aero.Core.Railway;


/// <inheritdoc />
/// <summary>
/// A default <see cref="Result{T, TError}"/> with <see cref="AeroError"/> as the error type.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public record Result<T> : Result<T, AeroError>
{
    /// <summary>
    /// Represents a successful result with a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="Value">The success value.</param>
    public new sealed record Ok(T Value) : Result<T>;

    /// <summary>
    /// Represents a failed result with an <see cref="AeroError"/>.
    /// </summary>
    /// <param name="Error">The error value.</param>
    public new sealed record Failure(AeroError Error) : Result<T>;

        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator Result<T>(T value) => new Ok(value);
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator Result<T>(AeroError error) => new Failure(error);

        /// <summary>
    /// From method.
    /// </summary>
public static Result<T> From(Result<T, AeroError> result)
    {
        return result switch
        {
            Result<T>.Ok ok => ok,
            Result<T>.Failure failure => failure,
            Result<T, AeroError>.Ok(var value) => new Ok(value),
            Result<T, AeroError>.Failure(var error) => new Failure(error),
            _ => new Failure(AeroError.CreateError("Unknown result state."))
        };
    }
}

/// <summary>
/// Represents a computation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="TError">The type of the error value in case of failure.</typeparam>
public abstract record Result<T, TError>
    where TError : AeroError
{
    //private Result() { } // Prevent external inheritance for exhaustiveness
        /// <summary>
    /// Represents a record for Ok.
    /// </summary>
public sealed record Ok(T Value) : Result<T, TError>;
        /// <summary>
    /// Represents a record for Failure.
    /// </summary>
public sealed record Failure(TError Error) : Result<T, TError>;

        /// <summary>
    /// Gets or sets the Is Success.
    /// </summary>
public bool IsSuccess => this is Ok;
        /// <summary>
    /// Gets or sets the Is Failure.
    /// </summary>
public bool IsFailure => this is Failure;

        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator Result<T, TError>(T value) => new Ok(value);
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator Result<T, TError>(TError error) => new Failure(error);

    //public static explicit operator T(Result<T, TError> result) =>
    //    result switch
    //    {
    //        Ok(var value) => value,
    //        Failure(var error) => throw new InvalidCastException($"Result was Failure: {error}"),
    //    };

    //public static explicit operator TError(Result<T, TError> result) =>
    //    result switch
    //    {
    //        Failure(var error) => error,
    //        Ok(var value) => throw new InvalidCastException($"Result was Ok: {value}"),
    //    };

        /// <summary>
    /// ToString method.
    /// </summary>
public override string ToString()
    {
        return this switch
        {
            Ok(var value) => $"{value.ToString()}",
            Failure(var error) => $"{error.ToString()}",
            _ => base.ToString() ?? "unknown error in Result.ToString()"
        };
    }
}

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the value that may be present.</typeparam>
public abstract record Option<T>
{
    private Option() { } // Prevent external inheritance

        /// <summary>
    /// Represents a record for Some.
    /// </summary>
public sealed record Some(T Value) : Option<T>;
        /// <summary>
    /// Represents a record for None.
    /// </summary>
public sealed record None : Option<T>;

    /// <summary>
    /// Gets a value indicating whether this Option contains a value (Some case).
    /// </summary>
    public bool IsSome => this is Some;

    /// <summary>
    /// Gets a value indicating whether this Option has no value (None case).
    /// </summary>
    public bool IsNone => this is None;

    /// <summary>
    /// Implicitly converts a value to Some if non-null, otherwise None.
    /// </summary>
    public static implicit operator Option<T>(T value) =>
        value is not null ? new Some(value) : new None();

    /// <summary>
    /// Implicitly converts the None struct to Option.None.
    /// </summary>
    public static implicit operator Option<T>(Railway.NoneType _) => new None();

    // todo - this allows unsafe casting and removes the point of ROP
    // if we want to enforce pattern matching, we can remove these explicit operators and require users to match on Some/None explicitly. This can help prevent accidental casts and make the intent clearer in code.
    // Optional: Remove if you want to enforce pattern matching
    //public static explicit operator T(Option<T> option) =>
    //    option switch
    //    {
    //        Some(var value) => value,
    //        None => throw new InvalidCastException("Cannot cast None to value"),
    //    };
}

/// <summary>
/// Represents the absence of a value, used primarily for type inference in generic contexts.
/// </summary>
public readonly struct NoneType
{
    //public static None Value => default;
}

