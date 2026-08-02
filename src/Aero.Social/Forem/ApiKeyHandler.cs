namespace Aero.Social.Forem;

/// <summary>
/// Represents a class for ForemApiKeyHandler.
/// </summary>
public class ForemApiKeyHandler : DelegatingHandler
{
        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // add stuff here - logging possibly
        
        return await base.SendAsync(request, cancellationToken);
    }
}