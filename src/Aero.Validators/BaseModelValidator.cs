using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

/// <summary>
/// Represents a class for BaseModelValidator.
/// </summary>
public abstract class BaseModelValidator<T>(IMemoryCache cache, ILogger<BaseModelValidator<T>> log)
    : AbstractValidator<T>
{
        /// <summary>
    /// cache.
    /// </summary>
protected readonly IMemoryCache cache = cache;
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<BaseModelValidator<T>> log = log;
}