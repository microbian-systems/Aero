namespace Aero.Core.Ai;

/// <summary>
/// Typed HttpClient for outbound LLM provider calls via LlmTornado.
/// Registered via AddHttpClient&lt;T&gt; — no automatic retry attached.
/// </summary>
public sealed class TornadoProviderClient(HttpClient httpClient)
{
        /// <summary>
    /// Gets or sets the Http Client.
    /// </summary>
public HttpClient HttpClient => httpClient;
}
