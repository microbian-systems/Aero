using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Avatars;

/// <summary>
/// Represents a class for AvatarGroup.
/// </summary>
public partial class AvatarGroup : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Group Size.
    /// </summary>
[Parameter]
    public AvatarSize GroupSize { get; set; } = AvatarSize.Sm;
}
