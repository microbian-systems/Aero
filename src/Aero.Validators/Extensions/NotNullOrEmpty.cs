using FluentValidation;
using FluentValidation.Validators;

namespace Aero.Validators.Extensions;

/// <summary>
/// Represents a class for NotNullOrEmpty.
/// </summary>
public class NotNullOrEmpty<T, TProperty> : AsyncPropertyValidator<T, TProperty>, INotNullOrEmpty
{
        /// <summary>
    /// GetDefaultMessageTemplate method.
    /// </summary>
protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return $"{nameof(NotNullOrEmpty<T, TProperty>)}: {errorCode}";
    }

        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name { get; }

        /// <summary>
    /// IsValidAsync method.
    /// </summary>
public override async Task<bool> IsValidAsync(ValidationContext<T> context, TProperty value, CancellationToken cancellation)
    {
            
        if (value is null || value is string)
            return !string.IsNullOrEmpty(value as string);

        return await Task.FromResult(true);
    }
}