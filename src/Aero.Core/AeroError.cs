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
    /// Represents an HTTP-related error with an associated status code and message.
    /// </summary>
    /// <param name="msg">The error message that describes the HTTP error.</param>
    /// <param name="code">The HTTP status code associated with the error.</param>
    public sealed record HttpRequest(HttpStatusCode code, string? msg = null) : AeroError;
    public static implicit operator AeroError(string msg) => new Error(msg);
    public static implicit operator AeroError((HttpStatusCode code, string msg) err) => new HttpRequest(err.code, err.msg);
    public static implicit operator AeroError(string[] errors) => new Validation(errors.ToImmutableList());
    public static implicit operator AeroError(ImmutableList<string> errors) => new Validation(errors);
    public static implicit operator AeroError(ImmutableArray<string> errors) => new Validation(errors.ToImmutableList());
    public static implicit operator AeroError(List<string> errors) => new Validation(errors.ToImmutableList());
    public static implicit operator AeroError(HashSet<string> errors) => new Validation(errors.ToImmutableList());

    public static Error CreateError(string msg) => new Error(msg);
    public static NotAllowed NotAllowedError(string msg) => new NotAllowed(msg);
    public static NotFound NotFoundError(string msg) => new NotFound(msg);
    public static Validation ValidationError(IEnumerable<string> errors) => new Validation(errors.ToImmutableList());
    public static Conflict ConflictError(string msg) => new Conflict(msg);
    public static Database DatabaseError(string msg) => new Database(msg);
    public static Unauthorized UnauthorizedError(string msg) => new Unauthorized(msg);
    public static Forbidden ForbiddenError(string msg) => new Forbidden(msg);
    public static Timeout TimeoutError(string msg) => new Timeout(msg);
    public static InvalidRequest InvalidRequestError(string msg) => new InvalidRequest(msg);
    public static BadRequest BadRequestError(string msg) => new BadRequest(msg);
    public static Exists ExistsError(string msg) => new Exists(msg);
    public static HttpRequest HttpRequestError(HttpStatusCode code, string? msg = null) => new HttpRequest(code, msg);
}

