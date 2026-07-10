using TUnit.Core;
using Aero.MerakiUI.Alerts;
using Bunit;
using Aero.MerakiUI.Alerts;

namespace Aero.MerakiUI.Tests.Alerts;

/// <summary>
/// Represents a class for AlertTests.
/// </summary>
public class AlertTests : BunitContext
{
        /// <summary>
    /// SuccessAlert_ShouldRenderCorrectClasses method.
    /// </summary>
[Test]
    public void SuccessAlert_ShouldRenderCorrectClasses()
    {
        var cut = Render<Alert>(parameters => parameters
            .Add(p => p.Type, AlertType.Success)
            .Add(p => p.Title, "Success")
            .AddChildContent("Your account was registered!")
        );

        cut.Find(".bg-emerald-500");
        Assert.Contains("Success", cut.Markup);
        Assert.Contains("Your account was registered!", cut.Markup);
    }

        /// <summary>
    /// InfoAlert_ShouldRenderCorrectClasses method.
    /// </summary>
[Test]
    public void InfoAlert_ShouldRenderCorrectClasses()
    {
        var cut = Render<Alert>(parameters => parameters
            .Add(p => p.Type, AlertType.Info)
            .Add(p => p.Title, "Info")
            .AddChildContent("This is info message")
        );

        cut.Find(".bg-blue-500");
        Assert.Contains("Info", cut.Markup);
        Assert.Contains("This is info message", cut.Markup);
    }

        /// <summary>
    /// WarningAlert_ShouldRenderCorrectClasses method.
    /// </summary>
[Test]
    public void WarningAlert_ShouldRenderCorrectClasses()
    {
        var cut = Render<Alert>(parameters => parameters
            .Add(p => p.Type, AlertType.Warning)
            .Add(p => p.Title, "Warning")
            .AddChildContent("This is warning message")
        );

        cut.Find(".bg-yellow-500");
        Assert.Contains("Warning", cut.Markup);
        Assert.Contains("This is warning message", cut.Markup);
}
}
