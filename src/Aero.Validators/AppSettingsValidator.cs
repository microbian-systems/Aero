using Aero.Core;
using Aero.Validators.Extensions;
using FluentValidation;

namespace Aero.Validators;

/// <summary>
/// Represents a class for AppSettingsValidator.
/// </summary>
public class AppSettingsValidator : AbstractValidator<AppSettings>
{
        /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsValidator"/> class.
    /// </summary>
public AppSettingsValidator()
    {
        RuleFor(x => x.Secret).NotNullOrEmpty()
            .WithMessage($"jwt secret (Secret) must not be empty");
        RuleFor(x => x.KeyVaultEndPoint).NotNullOrEmpty();
        RuleFor(x => x.AzureStorage).NotNullOrEmpty();
        RuleFor(x => x.ValidIssuers.Count).GreaterThanOrEqualTo(0);
        //RuleFor(x => x.AppInsightsKey).NotNullOrEmpty();
        //RuleFor(x => x.UseAzureStorage).NotNullOrEmpty();
        RuleFor(x => x.EnableMiniProfiler).NotNullOrEmpty();
        //RuleFor(x => x.AzureStorage.StorageKey).NotNullOrEmpty();
        //RuleFor(x => x.AzureStorage.StorageName).NotNullOrEmpty();
        //RuleFor(x => x.AzureStorage.ContainerName).NotNullOrEmpty();
    }
}