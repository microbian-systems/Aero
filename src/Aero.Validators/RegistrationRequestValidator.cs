using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

/// <summary>
/// Represents a class for RegistrationRequestValidator.
/// </summary>
public class RegistrationRequestValidator(IMemoryCache cache, ILogger<RegistrationRequestValidator> log)
    : BaseModelValidator<RegistrationRequestValidator>(cache, log);