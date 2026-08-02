using FluentValidation;
using FluentValidation.Validators;
using IPropertyValidator = FluentValidation.Validators.IPropertyValidator;

namespace Aero.Validators.Extensions;

/// <summary>
/// Defines an interface for INotNullOrEmpty.
/// </summary>
public interface INotNullOrEmpty : IPropertyValidator{}
/// <summary>
/// Represents a class for NotEmptyGuid.
/// </summary>
public class NotEmptyGuid<T, TProperty> : AsyncPropertyValidator<T, TProperty>, INotEmptyGuidValidator 
{
        /// <summary>
    /// IsValidAsync method.
    /// </summary>
public override async Task<bool> IsValidAsync(ValidationContext<T> context, TProperty value, CancellationToken cancellation)
    {
        var guid = value as Guid?;
        if (guid != null)
        {
            if (guid.Value == Guid.Empty)
                return false;
        }
        return await Task.FromResult(true);
    }

        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name { get; }
}