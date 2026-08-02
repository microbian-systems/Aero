using System.Diagnostics;

namespace Aero.Actors;


/// <summary>
/// Defines an interface for IAeroActor.
/// </summary>
public interface IAeroActor : IGrainWithIntegerCompoundKey;

/// <summary>
/// Base grain class for actors
/// </summary>
/// <param name="log">ILogger<T/> instance for logging</param>
public abstract class AeroActor(ILogger<AeroActor> log) : Grain, IAeroActor
{
        /// <summary>
    /// Span.
    /// </summary>
protected static readonly ActivitySource Span = new(nameof(AeroActor));

        /// <summary>
    /// OnActivateAsync method.
    /// </summary>
public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        using var activity = Span.StartActivity($"{GetType().Name}.Activated");
        activity?.SetTag("grain.id", this.GetGrainId().ToString());

        await base.OnActivateAsync(cancellationToken);

        log.LogInformation("Actor {Type} activated", GetType().Name);
    }

        /// <summary>
    /// OnDeactivateAsync method.
    /// </summary>
public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (reason.ReasonCode == DeactivationReasonCode.ShuttingDown)
        {
            log.LogInformation($"{this.GetType().Name} - {this.GetGrainId()} - actor deactivated");
            MigrateOnIdle();
        }
        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}

