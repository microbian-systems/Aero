namespace Aero.Core.Http;

public interface ITokenProvider
{
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}
