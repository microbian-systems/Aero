using Aero.Core;
using Aero.Core.Railway;

namespace Aero.Web.Extensions;


public static class MinimalApiResultMappingExtensions
{
    /// <summary>
    /// Converts a domain result object to an HTTP response result appropriate for the outcome for Minimal APIs.
    /// </summary>
    /// <remarks>The returned HTTP response is determined by the specific error type contained in the result.
    /// For example, not found errors are mapped to 404 responses, validation errors to 400 responses, and so on.
    /// Unexpected errors are mapped to a generic problem response.</remarks>
    /// <typeparam name="TError">The type representing error information. Must implement the IAeroError interface.</typeparam>
    /// <typeparam name="TValue">The type of the value returned on success.</typeparam>
    /// <param name="result">The result object to convert to an HTTP response. Represents either a successful value or a specific error.</param>
    /// <returns>A minimal API HTTP response result corresponding to the outcome of the provided result. Returns a success response if the
    /// result is successful, or an error response mapped to the specific error type.</returns>
    public static IResult ToResult<TError, TValue>(
        Result<TError, TValue> result)
        where TError : IAeroError
    {
        return result switch
        {
            Result<TError, TValue>.Ok(var value)
                => Results.Ok(value),

            Result<TError, TValue>.Failure(AeroError.NotFound nf)
                => Results.NotFound(nf.msg),

            Result<TError, TValue>.Failure(AeroError.Validation v)
                => Results.BadRequest(v.Errors),

            Result<TError, TValue>.Failure(AeroError.Unauthorized)
                => Results.Unauthorized(),

            Result<TError, TValue>.Failure(AeroError.Forbidden c)
                => Results.Forbid(),

            Result<TError, TValue>.Failure(AeroError.Conflict c)
                => Results.Conflict(c.msg),

            Result<TError, TValue>.Failure(AeroError.BadRequest c) 
                => Results.BadRequest(c.msg),

            Result<TError, TValue>.Failure(AeroError.NotAllowed c) 
                => Results.Problem(c.msg),

            Result<TError, TValue>.Failure(AeroError.Exists c) 
                => Results.Problem(c.msg),

            _ => Results.Problem("Unexpected error")
        };
    }
}
