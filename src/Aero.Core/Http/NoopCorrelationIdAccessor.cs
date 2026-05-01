namespace Aero.Core.Http;

/// <summary>
/// A no-op implementation of ICorrelationIdAccessor that returns null.
/// Suitable for environments without request context like WASM or MAUI.
/// </summary>
public sealed class NoopCorrelationIdAccessor : ICorrelationIdAccessor
{
    public string? CorrelationId => null;
}
