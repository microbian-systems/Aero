using Aero.Core;
using Aero.Core.Railway;
using Microsoft.Extensions.AI;

namespace Aero.Core.Ai;

/// <summary>
/// Defines an interface for IAiChatClientFactory.
/// </summary>
public interface IAiChatClientFactory
{
        /// <summary>
    /// CreateAsync method.
    /// </summary>
Task<Result<IChatClient>> CreateAsync(
        AiRuntimeSettings settings,
        CancellationToken cancellationToken = default);
}
