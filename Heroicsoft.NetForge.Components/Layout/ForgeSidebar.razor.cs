using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class ForgeSidebar
{
    [Parameter]
    public string? Brand { get; set; }

    [Parameter]
    public string BrandHref { get; set; } = "/";

    [Parameter]
    public bool EnhanceBrandNavigation { get; set; } = true;

    [Parameter]
    public string? LogoUrl { get; set; }

    [Parameter]
    public string? LogoDarkUrl { get; set; }

    [Parameter]
    public string? MarkUrl { get; set; }

    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>Replaces <see cref="Sections"/>. Used by the shell when a shared menu is switched on.</summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>Draws <see cref="Menu"/> after <see cref="Sections"/>.</summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>Pinned under the links, for example a user name and sign-out button. Hidden when the rail is collapsed.</summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }
}