using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders one sidebar section: an optional <c>bf-sidebar-heading</c>, then a vertical
/// <c>nav</c> of <see cref="ForgeNavLink"/> items. Items that list roles are omitted for other users.
/// Nested children are not expanded in this list.
/// </summary>
public partial class ForgeNavSectionView
{
    /// <summary>
    /// The section to render. A blank <see cref="ForgeNavSection.Heading"/> omits the heading.
    /// <see cref="ForgeNavSection.Items"/> are the links.
    /// </summary>
    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;
}