namespace Aero.Modular;

/// <summary>
/// Assembly-level attribute emitted by the per-module <see cref="WolverineHandlerGenerator"/>
/// to mark which type in the assembly provides the Wolverine handler registration callback.
/// The host generator reads this attribute from referenced assembly metadata to aggregate
/// handler registrations without scanning for <c>IWolverineHandler</c> interface implementations.
/// </summary>
/// <remarks>
/// This attribute is Wolverine-agnostic — it is only a marker type reference.
/// The actual registration type exposes a static <c>Register(WolverineOptions)</c> method.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class WolverineHandlersRegistrationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="WolverineHandlersRegistrationAttribute"/>.
    /// </summary>
    /// <param name="registrationType">
    /// The type that contains a static <c>Register(WolverineOptions)</c> method
    /// for including handler types.
    /// </param>
    public WolverineHandlersRegistrationAttribute(Type registrationType)
    {
        RegistrationType = registrationType;
    }

    /// <summary>
    /// The type with a static <c>Register(WolverineOptions)</c> method.
    /// </summary>
    public Type RegistrationType { get; }
}
