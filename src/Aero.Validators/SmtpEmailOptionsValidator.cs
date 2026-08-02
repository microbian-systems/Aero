using Aero.Core;
using Aero.Validators.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

/// <summary>
/// Represents a class for SmtpEmailOptionsValidator.
/// </summary>
public class SmtpEmailOptionsValidator : BaseModelValidator<SmtpEmailOptions>
{
        /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailOptionsValidator"/> class.
    /// </summary>
public SmtpEmailOptionsValidator(IMemoryCache cache, ILogger<SmtpEmailOptionsValidator> log) 
        : base(cache, log)
    {
        RuleFor(x => x.Host).NotNullOrEmpty();
        RuleFor(x => x.SenderEmail).NotNullOrEmpty();
    }
}