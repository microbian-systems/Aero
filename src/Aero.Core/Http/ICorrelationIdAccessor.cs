namespace Aero.Core.Http;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}
