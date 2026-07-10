using System.Net.Http.Headers;

namespace Aero.Core.Http;

/// <summary>
/// Represents a class for JwtTokenHandler.
/// </summary>
public sealed class JwtTokenHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

        /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenHandler"/> class.
    /// </summary>
public JwtTokenHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
