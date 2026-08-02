namespace Aero.Core.Validation;

/// <summary>
/// Represents a record for ValidationError.
/// </summary>
public sealed record ValidationError(string Field, string Message);


/// <summary>
/// Represents a class for ValidationException.
/// </summary>
public sealed class ValidationException(ValidationResult result)
    : Exception(result.ToString())
{
        /// <summary>
    /// Gets or sets the Result.
    /// </summary>
public ValidationResult Result { get; } = result;
}