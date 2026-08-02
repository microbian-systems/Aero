using Aero.Models;
using Aero.Validators.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

/// <summary>
/// Represents a class for ContactUsValidator.
/// </summary>
public class ContactUsValidator : BaseModelValidator<ContactUsModel>
{
        /// <summary>
    /// Initializes a new instance of the <see cref="ContactUsValidator"/> class.
    /// </summary>
public ContactUsValidator(IMemoryCache cache, ILogger<ContactUsValidator> log)
        : base(cache, log)
    {
        // todo - add test for proper email address
        // todo - add test for proper message length
        RuleFor(x => x.Name).NotNullOrEmpty();
        RuleFor(x => x.Email).NotNullOrEmpty();
        RuleFor(x => x.Message).NotNullOrEmpty();
    }
}