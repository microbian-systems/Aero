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
    public static IResult ToResult<TValue>(
        Result<TValue, AeroError> result)
    {
        return result switch
        {
            Result<TValue, AeroError>.Ok(var value)
                => Results.Ok(value),

            Result<TValue, AeroError>.Failure(AeroError.NotFound nf)
                => Results.NotFound(nf.msg),

            Result<TValue, AeroError>.Failure(AeroError.Validation v)
                => Results.BadRequest(v.Errors),

            Result<TValue, AeroError>.Failure(AeroError.Unauthorized)
                => Results.Unauthorized(),

            Result<TValue, AeroError>.Failure(AeroError.Forbidden c)
                => Results.Forbid(),

            Result<TValue, AeroError>.Failure(AeroError.Conflict c)
                => Results.Conflict(c.msg),

            Result<TValue, AeroError>.Failure(AeroError.BadRequest c) 
                => Results.BadRequest(c.msg),

            Result<TValue, AeroError>.Failure(AeroError.NotAllowed c) 
                => Results.Problem(c.msg),

            Result<TValue, AeroError>.Failure(AeroError.Exists c) 
                => Results.Problem(c.msg),

            // todo - in the AeroErrorMappingExtensions, return the message in the AeroError for the Problem() responses
            _ => Results.Problem("Unexpected error")
        };
    }
}
