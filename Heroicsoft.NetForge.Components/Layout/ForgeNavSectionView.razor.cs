using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders one sidebar section: an optional heading and its links. Items with roles are shown only to users in those roles.
/// </summary>
public partial class ForgeNavSectionView
{
    /// <summary>
    /// The section to draw. Required. A blank heading is omitted.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;
}