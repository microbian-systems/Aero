namespace Aero.Core.Validation;

/// <summary>
/// Represents a class for ValidationResult.
/// </summary>
public sealed class ValidationResult
{
        /// <summary>
    /// Valid.
    /// </summary>
public static readonly ValidationResult Valid = new([]);

        /// <summary>
    /// Gets or sets the Errors.
    /// </summary>
public IReadOnlyList<ValidationError> Errors { get; }
        /// <summary>
    /// Gets or sets the Is Valid.
    /// </summary>
public bool IsValid => Errors.Count == 0;

    internal ValidationResult(IEnumerable<ValidationError> errors)
        => Errors = errors.ToList().AsReadOnly();

    /// <summary>Throws a <see cref="ValidationException"/> if the result is invalid.</summary>
    public ValidationResult ThrowIfInvalid()
    {
        if (!IsValid) throw new ValidationException(this);
        return this;
    }

        /// <summary>
    /// ToString method.
    /// </summary>
public override string ToString()
        => IsValid ? "Valid" : string.Join("; ", Errors.Select(e => $"{e.Field}: {e.Message}"));
}