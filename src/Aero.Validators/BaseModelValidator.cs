using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

public abstract class BaseModelValidator<T>(IMemoryCache cache, ILogger<BaseModelValidator<T>> log)
    : AbstractValidator<T>
{
    protected readonly IMemoryCache cache = cache;
    protected readonly ILogger<BaseModelValidator<T>> log = log;
}