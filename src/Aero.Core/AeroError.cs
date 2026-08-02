using System.Collections.Immutable;
using System.Net;

namespace Aero.Core;

/// <summary>
/// Represents an error that occurs within the Aero framework.
/// </summary>
/// <remarks>Implement this interface to provide custom error information for Aero-based components or services.
/// This interface is intended to standardize error reporting and handling across Aero implementations.</remarks>
public interface IAeroError ;

/// <summary>
/// Represents a base type for standardized error results in application workflows.
/// </summary>
/// <remarks>Use derived records of this type to indicate specific error conditions, such as validation failures,
/// resource conflicts, or authorization issues. Each derived record provides additional context relevant to the error
/// scenario. This type is intended for use in error handling, result objects, or API responses to enable consistent
/// error reporting across the application.</remarks>
public abstract record AeroError : IAeroError
{
    /// <summary>
    /// Represents an generic error with an associated message.
    /// </summary>
    /// <param name="msg">The error message that describes the error condition. Cannot be null.</param>
    public sealed record Error(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that an operatio was cancelled (Task)
    /// </summary>
    /// <param name="msg">The message that describes the reason for the cancellation.</param>
    public sealed record Cancelled(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that an operation is not allowed.
    /// </summary>
    /// <param name="msg">The message that describes the reason the operation is not allowed.</param>
    public sealed record NotAllowed(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that a requested resource was not found.
    /// </summary>
    /// <param name="msg">The error message that describes the details of the not found condition.</param>
    public sealed record NotFound(string msg) : AeroError;
    /// <summary>
    /// Represents a validation error that contains a collection of error messages.
    /// </summary>
    /// <remarks>Use this type to encapsulate multiple validation failures when processing input or performing
    /// business logic. The error messages provide details about each validation issue encountered.</remarks>
    /// <param name="Errors">A read-only list of strings that describe the validation errors. Cannot be null or contain null elements.</param>
    public sealed record Validation(ImmutableList<string> Errors) : AeroError;
    /// <summary>
    /// Represents an error that occurs when a request cannot be completed due to a conflict with the current state of
    /// the resource.
    /// </summary>
    /// <param name="msg">The error message that describes the details of the conflict.</param>
    public sealed record Conflict(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs during a database operation.
    /// </summary>
    /// <param name="msg">The error message that describes the database error.</param>
    public sealed record Database(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that an operation was not authorized.
    /// </summary>
    /// <param name="msg">The error message that describes the unauthorized access.</param>
    public sealed record Unauthorized(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that the requested operation is forbidden due to insufficient permissions or
    /// access rights.
    /// </summary>
    /// <param name="msg">The error message that describes the reason the operation is forbidden.</param>
    public sealed record Forbidden(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs when an operation exceeds its allotted time limit.
    /// </summary>
    /// <param name="msg">The error message that describes the timeout condition.</param>
    public sealed record Timeout(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs when a request is invalid.
    /// </summary>
    /// <param name="msg">The error message that describes why the request is invalid.</param>
    public sealed record InvalidRequest(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs when a request is invalid or cannot be processed due to client-side issues.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the bad request.</param>
    public sealed record BadRequest(string msg) : AeroError;
    /// <summary>
    /// Represents an error indicating that an entity already exists.
    /// </summary>
    /// <param name="msg">The error message that describes the existence conflict.</param>
    public sealed record Exists(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs when a null reference is encountered.
    /// </summary>
    /// <param name="msg">The error message that describes the null reference condition.</param>
    public sealed record NullReferro(string msg) : AeroError;
    /// <summary>
    /// Represents an HTTP-related error with an associated status code and message.
    /// </summary>
    /// <param name="msg">The error message that describes the HTTP error.</param>
    /// <param name="code">The HTTP status code associated with the error.</param>
    public sealed record HttpRequest(HttpStatusCode code, string? msg = null) : AeroError;
    /// <summary>
    /// Represents an error related to configuration or settings.
    /// </summary>
    /// <param name="msg">The error message that describes the configuration issue.</param>
    public sealed record Configuration(string msg) : AeroError;

        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(string msg) => new Error(msg);
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError((HttpStatusCode code, string msg) err) => new HttpRequest(err.code, err.msg);
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(string[] errors) => new Validation(errors.ToImmutableList());
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(ImmutableList<string> errors) => new Validation(errors);
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(ImmutableArray<string> errors) => new Validation(errors.ToImmutableList());
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(List<string> errors) => new Validation(errors.ToImmutableList());
        /// <summary>
    /// Defines a conversion operator.
    /// </summary>
public static implicit operator AeroError(HashSet<string> errors) => new Validation(errors.ToImmutableList());

    /// <summary>
    /// Creates a new instance of the Error class with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that describes the error.</param>
    /// <returns>An Error object initialized with the specified error message.</returns>
    public static Error CreateError(string msg) => new Error(msg);
    /// <summary>
    /// Creates a new instance of the NotAllowed error with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason the operation is not allowed.</param>
    /// <returns>A NotAllowed error initialized with the provided message.</returns>
    public static NotAllowed NotAllowedError(string msg) => new NotAllowed(msg);
    /// <summary>
    /// Creates a new NotFound error result with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the not found result. Cannot be null.</param>
    /// <returns>A NotFound result containing the specified error message.</returns>
    public static NotFound NotFoundError(string msg) => new NotFound(msg);
    /// <summary>
    /// Creates a failed validation result containing the specified validation error messages.
    /// </summary>
    /// <param name="errors">A collection of error messages that describe the validation failures. Cannot be null.</param>
    /// <returns>A Validation instance representing a failed validation with the provided error messages.</returns>
    public static Validation ValidationError(IEnumerable<string> errors) => new Validation(errors.ToImmutableList());
    /// <summary>
    /// Creates a new Cancelled error instance with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the cancellation.</param>
    /// <returns>A Cancelled object initialized with the provided error message.</returns>
    public static Cancelled CancelledError(string msg) => new Cancelled(msg);
    /// <summary>
    /// Creates a new conflict error result with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the conflict.</param>
    /// <returns>A Conflict result containing the specified error message.</returns>
    public static Conflict ConflictError(string msg) => new Conflict(msg);
    /// <summary>
    /// Creates a new database instance representing an error state with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that describes the database error. Cannot be null.</param>
    /// <returns>A database instance initialized to represent the specified error condition.</returns>
    public static Database DatabaseError(string msg) => new Database(msg);
    /// <summary>
    /// Creates a new Unauthorized error result with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the unauthorized error. Cannot be null.</param>
    /// <returns>An Unauthorized result containing the specified error message.</returns>
    public static Unauthorized UnauthorizedError(string msg) => new Unauthorized(msg);
    /// <summary>
    /// Creates a new Forbidden error result with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the forbidden result. Cannot be null.</param>
    /// <returns>A Forbidden result containing the specified error message.</returns>
    public static Forbidden ForbiddenError(string msg) => new Forbidden(msg);
    /// <summary>
    /// Creates a new Timeout instance representing a timeout error with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the timeout condition.</param>
    /// <returns>A Timeout instance initialized with the specified error message.</returns>
    public static Timeout TimeoutError(string msg) => new Timeout(msg);
    /// <summary>
    /// Creates a new instance of the InvalidRequest error with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the invalid request.</param>
    /// <returns>An InvalidRequest object initialized with the specified error message.</returns>
    public static InvalidRequest InvalidRequestError(string msg) => new InvalidRequest(msg);
    /// <summary>
    /// Creates a new BadRequest error result with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the bad request. Cannot be null.</param>
    /// <returns>A BadRequest result containing the specified error message.</returns>
    public static BadRequest BadRequestError(string msg) => new BadRequest(msg);
    /// <summary>
    /// Creates a new instance of the Exists error type with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that describes the existence error.</param>
    /// <returns>An Exists object initialized with the provided error message.</returns>
    public static Exists ExistsError(string msg) => new Exists(msg);
    /// <summary>
    /// Creates a new instance of the NullReferro exception with a specified error message.
    /// </summary>
    /// <param name="msg">The error message that describes the reason for the exception. Cannot be null.</param>
    /// <returns>A new NullReferro instance initialized with the specified error message.</returns>
    public static NullReferro NullReferenceError(string msg) => new NullReferro(msg);
    /// <summary>
    /// Creates a configuration instance that represents an error state with the specified message.
    /// </summary>
    /// <param name="msg">The error message that describes the configuration issue. Cannot be null.</param>
    /// <returns>A configuration instance initialized to represent an error with the provided message.</returns>
    public static Configuration ConfigurationError(string msg) => new Configuration(msg);
    /// <summary>
    /// Creates a new error instance representing an HTTP request failure with the specified status code and message.
    /// </summary>
    /// <param name="code">The HTTP status code associated with the error.</param>
    /// <param name="msg">The error message that describes the HTTP request failure.</param>
    /// <returns>An instance of AeroError representing the HTTP request error with the provided status code and message.</returns>
    public static AeroError HttpRequestError(HttpStatusCode code, string msg) => new HttpRequest(code, msg);
}

