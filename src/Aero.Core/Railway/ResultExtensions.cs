namespace Aero.Core.Railway;

using Aero.Core;

/// <summary>
/// Provides extension methods for working with <see cref="Result{TValue, TError}"/> types,
/// enabling fluent, railway-oriented programming patterns.
/// </summary>
/// <remarks>
/// <para>
/// This class contains the core functional operations for the Result type, implementing
/// the monadic patterns that enable composable error handling. Each method provides
/// a way to transform, chain, or extract values from Results while maintaining the
/// railway-oriented flow.
/// </para>
/// <para>
/// <b>Monadic Lifting in Detail:</b>
/// The Map and Bind operations represent "lifting" functions into the Result monadic context:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <b>Map (Functor lift):</b> Lifts a function <c>T -&gt; U</c> to operate on <c>Result&lt;T, E&gt; -&gt; Result&lt;U, E&gt;</c>.
/// If the Result is Ok, the function is applied; if Failure, the error propagates unchanged.
/// This preserves the error type while transforming the success value.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>Bind (Monadic lift):</b> Lifts a function <c>T -&gt; Result&lt;U, E&gt;</c> to operate on 
/// <c>Result&lt;T, E&gt; -&gt; Result&lt;U, E&gt;</c>. This is used for chaining operations that themselves
/// can fail, flattening nested Results automatically.
/// </description>
/// </item>
/// </list>
/// <para>
/// These operations allow you to compose complex workflows from simple functions,
/// with error handling happening automatically via the "failure rail" of the railway pattern.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Chaining multiple operations with automatic error handling
/// var result = ParseInt("42")
///     .Map(n => n * 2)                    // Transforms 42 to 84
///     .Bind(n => Divide(100, n))          // Chains another Result-returning operation
///     .Filter(n => n > 1, new ValidationError("Too small"))    // Converts to Failure if condition fails
///     .Match(
///         value => $"Result: {value}",
///         error => $"Error: {error.Message}"
///     );
/// </code>
/// </example>
public static partial class ResultExtensions
{
    /// <summary>
    /// Converts the legacy two-parameter AeroError result into the default <see cref="Result{T}"/> shape.
    /// </summary>
    public static Result<TValue> ToDefaultResult<TValue>(this Result<TValue, AeroError> result) =>
        Result<TValue>.From(result);

    /// <summary>
    /// Gets a value indicating whether the Result is Ok.
    /// </summary>
    public static bool IsSuccess<TValue, TError>(this Result<TValue, TError> r) where TError : AeroError => r is Result<TValue, TError>.Ok;

    /// <summary>
    /// Gets a value indicating whether the Result is Failure.
    /// </summary>
    public static bool IsFailure<TValue, TError>(this Result<TValue, TError> r) where TError : AeroError => r is Result<TValue, TError>.Failure;

    /// <summary>
    /// Transforms the success value of a Result using the provided function, preserving any error.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the input success value.</typeparam>
    /// <typeparam name="TOut">The type of the output success value.</typeparam>
    /// <param name="r">The Result to transform.</param>
    /// <param name="f">The function to apply to the success value.</param>
    /// <returns>
    /// A new Result containing the transformed value if the input was Ok;
    /// otherwise, a Result containing the original error.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Map implements the functor pattern, "lifting" a plain function into the Result context.
    /// It allows you to transform success values while automatically bypassing the transformation
    /// when the Result is a Failure (the error stays on the "failure rail").
    /// </para>
    /// <para>
    /// This is analogous to LINQ's Select method for IEnumerable, or Option.Map for the Option type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, ValidationError&gt; result = 42;
    /// Result&lt;string, ValidationError&gt; mapped = result.Map(n => $"Number: {n}");
    /// // mapped is Ok("Number: 42")
    /// 
    /// Result&lt;int, ValidationError&gt; failure = new ValidationError("Error");
    /// Result&lt;string, ValidationError&gt; mapped2 = failure.Map(n => $"Number: {n}");
    /// // mapped2 is still Failure("Error") - the function was not called
    /// </code>
    /// </example>
    public static Result<TOut, TError> Map<TValue, TError, TOut>(
        this Result<TValue, TError> r, Func<TValue, TOut> f) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => new Result<TOut, TError>.Ok(f(value)),
            Result<TValue, TError>.Failure(var error) => new Result<TOut, TError>.Failure(error),
        };

    /// <summary>
    /// Asynchronously transforms the success value of a Result using the provided async function.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the input success value.</typeparam>
    /// <typeparam name="TOut">The type of the output success value.</typeparam>
    /// <param name="r">The Result to transform.</param>
    /// <param name="f">The async function to apply to the success value.</param>
    /// <returns>
    /// A task that resolves to a Result containing the transformed value if the input was Ok;
    /// otherwise, a Result containing the original error.
    /// </returns>
    /// <remarks>
    /// This is the asynchronous version of <see cref="Map{TError, TValue, TOut}"/>.
    /// It enables lifting async operations into the Result railway, which is essential
    /// for I/O-bound operations like database queries or HTTP calls.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, ValidationError&gt; result = 42;
    /// Result&lt;User, ValidationError&gt; userResult = await result.MapAsync(async id => 
    ///     await dbContext.Users.FindAsync(id));
    /// </code>
    /// </example>
    public static async Task<Result<TOut, TError>> MapAsync<TValue, TError, TOut>(
        this Result<TValue, TError> r, Func<TValue, Task<TOut>> f) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => new Result<TOut, TError>.Ok(await f(value)),
            Result<TValue, TError>.Failure(var error) => new Result<TOut, TError>.Failure(error),
        };

    /// <summary>
    /// Transforms the error value of a Result using the provided function, preserving any success value.
    /// </summary>
    /// <typeparam name="TError">The type of the input error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TErrorOut">The type of the output error.</typeparam>
    /// <param name="r">The Result to transform.</param>
    /// <param name="f">The function to apply to the error value.</param>
    /// <returns>
    /// A new Result containing the transformed error if the input was Failure;
    /// otherwise, a Result containing the original success value.
    /// </returns>
    /// <remarks>
    /// <para>
    /// MapError is the error-channel equivalent of Map. It allows you to transform error values,
    /// for example, to convert low-level errors into user-friendly messages or to unify error types
    /// when combining Results from different sources.
    /// </para>
    /// <para>
    /// This operation stays on the "failure rail" and only transforms errors, leaving success values untouched.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, DatabaseError&gt; result = new DatabaseError("DB error");
    /// Result&lt;int, ValidationError&gt; mapped = result.MapError(ex => new ValidationError($"Database error: {ex.Message}"));
    /// // mapped is Failure("Database error: DB error")
    /// 
    /// Result&lt;int, DatabaseError&gt; success = 42;
    /// Result&lt;int, ValidationError&gt; mapped2 = success.MapError(ex => new ValidationError(ex.Message));
    /// // mapped2 is still Ok(42)
    /// </code>
    /// </example>
    public static Result<TValue, TErrorOut> MapError<TValue, TError, TErrorOut>(
        this Result<TValue, TError> r, Func<TError, TErrorOut> f)
        where TError : AeroError
        where TErrorOut : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => new Result<TValue, TErrorOut>.Ok(value),
            Result<TValue, TError>.Failure(var error) => new Result<TValue, TErrorOut>.Failure(f(error)),
        };

    /// <summary>
    /// Chains a Result-returning function to the success value of a Result, flattening the nested Results.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the input success value.</typeparam>
    /// <typeparam name="TOut">The type of the output success value.</typeparam>
    /// <param name="r">The Result to chain from.</param>
    /// <param name="f">The function that takes a success value and returns a new Result.</param>
    /// <returns>
    /// The Result returned by <paramref name="f"/> if the input was Ok;
    /// otherwise, a Result containing the original error.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Bind implements the monadic pattern, "lifting" a Result-returning function into the Result context.
    /// Unlike Map, which would create a <c>Result&lt;E, Result&lt;E, T&gt;&gt;</c>, Bind flattens the structure
    /// to just <c>Result&lt;E, T&gt;</c>. This is essential for chaining operations where each step can fail.
    /// </para>
    /// <para>
    /// In railway-oriented programming terms, Bind allows you to switch tracks: if the current Result
    /// is on the success rail, it executes the next operation; if on the failure rail, it bypasses
    /// the operation and propagates the error forward.
    /// </para>
    /// <para>
    /// This is analogous to LINQ's SelectMany (monadic bind) for IEnumerable, or Option.Bind.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, ValidationError&gt; ParseInt(string s) => 
    ///     int.TryParse(s, out var n) ? n : new ValidationError("Parse error");
    /// 
    /// Result&lt;int, ValidationError&gt; Divide(int a, int b) =>
    ///     b == 0 ? new ValidationError("Division by zero") : a / b;
    /// 
    /// var result = ParseInt("100")
    ///     .Bind(n => Divide(n, 2))   // Chains the division operation
    ///     .Bind(n => Divide(n, 5));  // Chains another division
    /// // result is Ok(10)
    /// 
    /// var failure = ParseInt("100")
    ///     .Bind(n => Divide(n, 0));  // Division fails
    /// // failure is Failure("Division by zero")
    /// </code>
    /// </example>
    public static Result<TOut, TError> Bind<TValue, TError, TOut>(
        this Result<TValue, TError> r, Func<TValue, Result<TOut, TError>> f) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => f(value),
            Result<TValue, TError>.Failure(var error) => new Result<TOut, TError>.Failure(error),
        };

    /// <summary>
    /// Asynchronously chains a Result-returning function to the success value of a Result.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the input success value.</typeparam>
    /// <typeparam name="TOut">The type of the output success value.</typeparam>
    /// <param name="r">The Result to chain from.</param>
    /// <param name="f">The async function that takes a success value and returns a Task of Result.</param>
    /// <returns>
    /// A task that resolves to the Result returned by <paramref name="f"/> if the input was Ok;
    /// otherwise, a Result containing the original error.
    /// </returns>
    /// <remarks>
    /// This is the asynchronous version of <see cref="Bind{TError, TValue, TOut}"/>.
    /// It enables chaining async operations in the Result railway, which is crucial for
    /// composing I/O-bound workflows like API calls, database operations, or file access.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;int, ValidationError&gt; userId = 42;
    /// Result&lt;Order[], ValidationError&gt; orders = await userId.BindAsync(async id =>
    /// {
    ///     var user = await userService.GetByIdAsync(id);
    ///     return user.IsActive 
    ///         ? await orderService.GetForUserAsync(id)
    ///         : (Result&lt;Order[], ValidationError&gt;)new ValidationError("User inactive");
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<TOut, TError>> BindAsync<TValue, TError, TOut>(
        this Result<TValue, TError> r, Func<TValue, Task<Result<TOut, TError>>> f) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => await f(value),
            Result<TValue, TError>.Failure(var error) => new Result<TOut, TError>.Failure(error),
        };

    /// <summary>
    /// Applies one of two functions based on whether the Result is Ok or Failure,
    /// collapsing the Result to a single value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="T">The type of the output value.</typeparam>
    /// <param name="r">The Result to match against.</param>
    /// <param name="onOk">The function to apply if the Result is Ok.</param>
    /// <param name="onFailure">The function to apply if the Result is Failure.</param>
    /// <returns>
    /// The result of applying <paramref name="onOk"/> to the success value if Ok,
    /// or the result of applying <paramref name="onFailure"/> to the error if Failure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Match is the elimination principle for Results - it allows you to exit the railway
    /// and handle both cases, producing a single value. This is typically used at the end
    /// of a pipeline to extract the final result or handle errors appropriately.
    /// </para>
    /// <para>
    /// Unlike GetOrElse which only provides a fallback, Match allows you to transform both
    /// the success and error cases into a common type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string, int&gt; result = 42;
    /// string message = result.Match(
    ///     value => $"Success: {value}",
    ///     error => $"Failed: {error}"
    /// );
    /// // message is "Success: 42"
    /// 
    /// Result&lt;string, int&gt; failure = "Not found";
    /// int code = failure.Match(
    ///     value => value,
    ///     error => error == "Not found" ? 404 : 500
    /// );
    /// // code is 404
    /// </code>
    /// </example>
    public static T Match<TValue, TError, T>(
        this Result<TValue, TError> r,
        Func<TValue, T> onOk,
        Func<TError, T> onFailure) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => onOk(value),
            Result<TValue, TError>.Failure(var error) => onFailure(error),
        };

    /// <summary>
    /// Asynchronously applies one of two async functions based on whether the Result is Ok or Failure.
    /// </summary>
    public static async Task<TResult> MatchAsync<TValue, TError, TResult>(
        this Result<TValue, TError> result,
        Func<TValue, Task<TResult>> onOk,
        Func<TError, Task<TResult>> onFailure) where TError : AeroError =>
        result switch
        {
            Result<TValue, TError>.Ok(var value) => await onOk(value),
            Result<TValue, TError>.Failure(var error) => await onFailure(error),
        };

    /// <summary>
    /// Filters a Result based on a predicate, converting Ok to Failure if the predicate returns false.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to filter.</param>
    /// <param name="predicate">The condition that the success value must satisfy.</param>
    /// <param name="errorIfFalse">The error to use if the predicate returns false.</param>
    /// <returns>
    /// The original Result if it was Ok and the predicate returns true;
    /// a Failure containing <paramref name="errorIfFalse"/> if Ok but predicate returns false;
    /// the original Failure if it was already a Failure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Filter allows you to add validation constraints to the success rail. If a value
    /// doesn't meet the criteria, it switches from the success rail to the failure rail.
    /// </para>
    /// <para>
    /// This is the Result equivalent of Option's Filter operation. It's useful for
    /// adding runtime checks to your pipeline without breaking the fluent chain.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string, int&gt; result = 42;
    /// Result&lt;string, int&gt; filtered = result.Filter(n => n > 50, "Number too small");
    /// // filtered is Failure("Number too small")
    /// 
    /// Result&lt;string, int&gt; result2 = 75;
    /// Result&lt;string, int&gt; filtered2 = result2.Filter(n => n > 50, "Number too small");
    /// // filtered2 is Ok(75)
    /// 
    /// Result&lt;string, int&gt; failure = "Parse error";
    /// Result&lt;string, int&gt; filtered3 = failure.Filter(n => n > 0, "Negative");
    /// // filtered3 is still Failure("Parse error")
    /// </code>
    /// </example>
    public static Result<TValue, TError> Filter<TValue, TError>(
        this Result<TValue, TError> r,
        Func<TValue, bool> predicate,
        TError errorIfFalse) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) when predicate(value) => r,
            Result<TValue, TError>.Ok => new Result<TValue, TError>.Failure(errorIfFalse),
            Result<TValue, TError>.Failure => r,
        };

    /// <summary>
    /// Returns the success value if the Result is Ok; otherwise, returns the provided fallback value.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to extract from.</param>
    /// <param name="fallback">The value to return if the Result is a Failure.</param>
    /// <returns>
    /// The success value if the Result is Ok; otherwise, <paramref name="fallback"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// GetOrElse provides a simple way to extract a value from a Result with a default fallback.
    /// Unlike Match, this doesn't allow transforming the error - it simply provides a default.
    /// </para>
    /// <para>
    /// This is the Result equivalent of Option's GetOrElse. It's useful when you want to
    /// provide a sensible default and don't need to differentiate between different error cases.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string, int&gt; result = 42;
    /// int value = result.GetOrElse(0);
    /// // value is 42
    /// 
    /// Result&lt;string, int&gt; failure = "Error";
    /// int value2 = failure.GetOrElse(0);
    /// // value2 is 0 (the fallback)
    /// </code>
    /// </example>
    public static TValue GetOrElse<TValue, TError>(
        this Result<TValue, TError> r, TValue fallback) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => value,
            Result<TValue, TError>.Failure => fallback,
        };

    /// <summary>
    /// Returns the success value if the Result is Ok; otherwise, returns the result of the fallback function.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to extract from.</param>
    /// <param name="fallback">A function that takes the error and returns a fallback value.</param>
    /// <returns>
    /// The success value if the Result is Ok;
    /// otherwise, the result of calling <paramref name="fallback"/> with the error.
    /// </returns>
    /// <remarks>
    /// This overload allows the fallback value to be computed based on the specific error,
    /// providing more flexibility than the simple value fallback. It's useful when different
    /// errors should result in different default values.
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string, int&gt; failure = "Not found";
    /// int value = failure.GetOrElse(err => err switch
    /// {
    ///     "Not found" => 404,
    ///     "Unauthorized" => 401,
    ///     _ => 500
    /// });
    /// // value is 404
    /// </code>
    /// </example>
    public static TValue GetOrElse<TValue, TError>(
        this Result<TValue, TError> r, Func<TError, TValue> fallback) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => value,
            Result<TValue, TError>.Failure(var error) => fallback(error),
        };

    /// <summary>
    /// Returns the success value if the Result is Ok; otherwise, throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to extract from.</param>
    /// <param name="message">Optional custom message for the exception. If null, a default message including the error is used.</param>
    /// <returns>The success value if the Result is Ok.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Result is a Failure. The message includes either the custom message or the error value.
    /// </exception>
    /// <remarks>
    /// <para>
    /// GetOrThrow provides a way to forcefully extract a value, converting a Failure into an exception.
    /// This should be used sparingly, as it defeats the purpose of railway-oriented programming.
    /// </para>
    /// <para>
    /// Use this when you're at a boundary where you must throw (e.g., for compatibility with
    /// exception-based APIs) or when a Failure truly represents an exceptional, unrecoverable condition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;string, User&gt; result = user;
    /// User user = result.GetOrThrow("User must be present");
    /// 
    /// Result&lt;string, User&gt; failure = "Not authenticated";
    /// User user2 = failure.GetOrThrow();  // Throws InvalidOperationException: "Result was Failure: Not authenticated"
    /// </code>
    /// </example>
    public static TValue GetOrThrow<TValue, TError>(
        this Result<TValue, TError> r, string? message = null) where TError : AeroError =>
        r switch
        {
            Result<TValue, TError>.Ok(var value) => value,
            Result<TValue, TError>.Failure(var error) =>
                throw new InvalidOperationException(message ?? $"Result was Failure: {error}"),
        };

    /// <summary>
    /// Executes a side-effect action on the success value without changing the Result.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to tap into.</param>
    /// <param name="action">The side-effect action to execute on the success value.</param>
    /// <returns>The original Result unchanged.</returns>
    /// <remarks>
    /// <para>
    /// Tap allows you to perform side effects (like logging) in the middle of a pipeline
    /// without breaking the chain. The Result is returned unchanged, making it transparent
    /// to the rest of the pipeline.
    /// </para>
    /// <para>
    /// This is useful for debugging, logging, or triggering notifications without affecting
    /// the data flow. The action is only executed if the Result is Ok.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = ParseOrder(json)
    ///     .Tap(order => logger.LogInformation("Parsed order {OrderId}", order.Id))
    ///     .Bind(order => ValidateOrder(order))
    ///     .Tap(order => metrics.RecordOrderValidated())
    ///     .Map(order => CalculateTotal(order));
    /// </code>
    /// </example>
    public static Result<TValue, TError> Tap<TValue, TError>(
        this Result<TValue, TError> r, Action<TValue> action) where TError : AeroError
    {
        if (r is Result<TValue, TError>.Ok(var value)) action(value);
        return r;
    }

    /// <summary>
    /// Executes a side-effect action on the error value without changing the Result.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="r">The Result to tap into.</param>
    /// <param name="action">The side-effect action to execute on the error value.</param>
    /// <returns>The original Result unchanged.</returns>
    /// <remarks>
    /// <para>
    /// TapError is the error-channel equivalent of Tap. It allows you to perform side effects
    /// on errors (like logging failures) without affecting the Result or breaking the pipeline.
    /// </para>
    /// <para>
    /// This is useful for error logging, metrics collection, or failure notifications.
    /// The action is only executed if the Result is a Failure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = ParseOrder(json)
    ///     .TapError(err => logger.LogError("Failed to parse: {Error}", err))
    ///     .Bind(order => ValidateOrder(order))
    ///     .TapError(err => metrics.RecordValidationFailure())
    ///     .Map(order => CalculateTotal(order));
    /// </code>
    /// </example>
    public static Result<TValue, TError> TapError<TValue, TError>(
        this Result<TValue, TError> r, Action<TError> action) where TError : AeroError
    {
        if (r is Result<TValue, TError>.Failure(var error)) action(error);
        return r;
    }
}
