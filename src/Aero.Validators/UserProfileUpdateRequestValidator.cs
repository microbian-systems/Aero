using Aero.Models;
using Aero.Models.Entities;
using Aero.Validators.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

/// <summary>
/// Represents a class for UserProfileUpdateRequestValidator.
/// </summary>
public class UserProfileUpdateRequestValidator : BaseModelValidator<UserProfileUpdateRequest>
{
        /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileUpdateRequestValidator"/> class.
    /// </summary>
public UserProfileUpdateRequestValidator(IMemoryCache cache,
        ILogger<BaseModelValidator<UserProfileUpdateRequest>> log)
        : base(cache, log)
    {
        RuleFor(x => x.Id).NotNullOrEmpty();
    }

        /// <summary>
    /// NotBeEmptyGuid method.
    /// </summary>
protected bool NotBeEmptyGuid(Guid? guid) => 
        guid == null || (guid.HasValue && guid.Value != Guid.Empty ? true : false);
}
    
/// <summary>
/// Represents a class for UserProfileValidator.
/// </summary>
public class UserProfileValidator : BaseModelValidator<AeroUserProfile>
{
        /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileValidator"/> class.
    /// </summary>
public UserProfileValidator(IMemoryCache cache,
        ILogger<UserProfileValidator> log)
        : base(cache, log)
    {
        RuleFor(x => x.Id).NotNullOrEmpty();
    }
}