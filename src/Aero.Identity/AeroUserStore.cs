using Aero.Identity.Models;
using Marten;

namespace Aero.Identity;

/// <summary>
/// AeroDB store for users.
/// </summary>
/// <typeparam name="TUser">The user type.</typeparam>
public class AeroUserStore<TUser> : AeroUserStore<TUser, AeroRole>
    where TUser : AeroUser, new()
{
    /// <summary>
    /// Initializes a new instance of the AeroUserStore.
    /// </summary>
    /// <param name="session">The AeroDB session.</param>
    public AeroUserStore(IDocumentSession session) : base(session)
    {
    }
}