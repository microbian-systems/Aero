using TUnit.Core;
using Aero.MerakiUI.Skeletons;
using Bunit;
using Aero.MerakiUI.Skeletons;

namespace Aero.MerakiUI.Tests.Skeletons;

/// <summary>
/// Represents a class for SkeletonTests.
/// </summary>
public class SkeletonTests : BunitContext
{
        /// <summary>
    /// SkeletonCard_ShouldRenderPulse method.
    /// </summary>
[Test]
    public void SkeletonCard_ShouldRenderPulse()
    {
        var cut = Render<SkeletonCard>();
        cut.Find(".animate-pulse");
}
}
