namespace Aero.Core.Http;

/// <summary>
/// Defines an interface for ITokenProvider.
/// </summary>
public interface ITokenProvider
{
        /// <summary>
    /// GetAccessTokenAsync method.
    /// </summary>
ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}
