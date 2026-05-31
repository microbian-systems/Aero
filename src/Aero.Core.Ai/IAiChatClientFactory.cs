using Aero.Core;
using Aero.Core.Railway;
using Microsoft.Extensions.AI;

namespace Aero.Core.Ai;

public interface IAiChatClientFactory
{
    Task<Result<IChatClient, AeroError>> CreateAsync(
        AiRuntimeSettings settings,
        CancellationToken cancellationToken = default);
}
