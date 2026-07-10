using FluentValidation;

namespace Aero.Validators.Extensions;

// todo - fix empty guid validator


    
/// <summary>
/// Represents a class for ValidatorExtensions.
/// </summary>
public static class ValidatorExtensions
{
        /// <summary>
    /// GuidNotEmpty method.
    /// </summary>
public static IRuleBuilderOptions<T, TProperty> GuidNotEmpty<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetAsyncValidator(new NotEmptyGuid<T, TProperty>());
    }
    
        /// <summary>
    /// NotNullOrEmpty method.
    /// </summary>
public static IRuleBuilderOptions<T, TProperty> NotNullOrEmpty<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetAsyncValidator(new NotNullOrEmpty<T, TProperty>());
    }
}