using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public sealed record Validation(IReadOnlyList<string> Errors) : AeroError;
    /// <summary>
    /// Represents an error that occurs when a request cannot be completed due to a conflict with the current state of
    /// the resource.
    /// </summary>
    /// <param name="msg">The error message that describes the details of the conflict.</param>
    public sealed record Conflict(string msg) : AeroError;
    /// <summary>
    /// Represents an error of unknown type with an associated message.
    /// </summary>
    /// <param name="msg">The error message that describes the unknown error.</param>
    public sealed record Unknown(string msg) : AeroError;
    /// <summary>
    /// Represents an error that occurs during a database operation.
    /// </summary>
    /// <param name="msg">The error message that describes the database error.</param>
    public sealed record DatabaseError(string msg) : AeroError;
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
}

